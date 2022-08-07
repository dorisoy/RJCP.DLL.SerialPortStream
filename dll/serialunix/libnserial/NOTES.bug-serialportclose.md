# BUGS on Closing the Serial Port

This file contains some notes during the development of nserial software
library.

## Opening and Closing the Serial Port

Some drivers don't behave too well if you exit your program without closing the
serial port (with either the serial_close() or serial_terminate() call).

The test case involves setting up two serial ports with a single process. A
large amount of data is sent (up to 256000 bytes) and only 1024 bytes is read.
The program exits without closing the FD associated with the serial port.

The test case that originally showed this problem was SerialSendReceiveTest,
found in the file 'sendreceivesimple.cpp'. It would send data and stop after
reading only 1024 bytes, with the process exiting immediately afterwards.

On the next start, one could read data until it was flushed, but as soon as a
new byte arrived, there was a large amount of NUL data that was also received,
which never appeared on the serial port. One had to send a byte to the serial
port receiving the data, and then flush for the data to be synchronised again.

So the original code in DoTransfer is pasted below, with the workaround
commented out (serial_write()).

```c
int SerialReadWrite::DoTransfer(Buffer *sendBuffer)
{
  if (sendBuffer == NULL) {
    throw std::invalid_argument("sendBuffer may not be NULL");
  }

  m_writebuff = sendBuffer;

  char *rbuff = m_readbuff->GetBuffer();
  char *wbuff = m_writebuff->GetBuffer();

  int error = 0;

  bool writefinished;
  bool readfinished;
  serialevent_t event;

  // Read all data from the serial port.
  writefinished = false;
  readfinished = false;
  char tmpbuff[256];

  // Bug found for FTDI serial ports used to receive data. We have to write a
  // single byte to the FTDI device, so that we can reliably flush the data in
  // the FTDI device. If we don't, we might read some data and look like we
  // flush, but we read a lot of zero's causing data to be offset. As a
  // workaround, if the user removes the FTDI and then reinserts it, the write
  // isn't needed.
  //
  // To create the issue, we need to write to the receiving port, read only a
  // portion of that data and kill the process. On restart of the process,
  // flushing doesn't work unless a single byte is first written. The problem
  // canot be reproduced by simply writing a portion, closing the port, then
  // reopening the port without closing the process in-between. See test case
  // "SendReceiveOpenCloseOpenClose" that attempts to do this. See the test
  // case "SendReceiveFinalTestCase" that does create this problem, on the
  // second run of the test program.
  //
  // To create the condition where not writing a single byte causes flushing
  // to not work properly, we need to write from the remote side before the
  // serial port is open for read. For example, a previous test case could
  // write 1000 bytes, and end without reading it. That has the same effect.
  //
  // Tested to work properly:
  // * 16550A standard UART for input
  // * PL2303RA UART for input
  //
  // Tested to not work properly:
  // * FTDI for input
  // * PL2303H for input
  //
  // Removing the write and replacing also with tcflush(fd, TCIOFLUSH) doesn't
  // work either.

  //serial_write(m_writehandle, tmpbuff, 1);

  // We need a small delay before resetting. Instead of actually implementing
  // this delay, we will continue to read doing the flush manually, as it's
  // faster (with the assumption that we know that data isn't being sent to us
  // at that time).
  //serial_reset(m_writehandle);
  //serial_reset(m_readhandle);
  while (!readfinished) {
    event = serial_waitforevent(m_readhandle, READEVENT, 500);
    if (event == READEVENT) {
      int br = serial_read(m_readhandle, tmpbuff, 256);
      //std::cout << br << " = read() for flush\n";
    } else {
      readfinished = true;
    }
  }
```

This problem was observed on FTDI USB to Serial adapters and PL2303H USB Serial
Adapters. It wasn't visible with PL2303RA or a normal 16550A COM port.

Even though I believe this is a bug in the Linux kernel (tested was Ubuntu
14.04.3 with kernel:

`Linux leon-ubuntu 3.19.0-49-generic #55~14.04.1-Ubuntu SMP Fri Jan 22 11:23:34
UTC 2016 i686 i686 i686 GNU/Linux`

the solution is simple. Before the program ends (or the test case executable
exits), ensure that you close the serial port. Then your code will work as
expected. The workaround was removed, along with the comments and this note was
made in its place as a bit of a reminder.
