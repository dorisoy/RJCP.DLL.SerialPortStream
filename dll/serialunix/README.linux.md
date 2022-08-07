# README for Linux

This package will compile and work on Linux. This page contains some notes
while testing.

## Behaviour of various chipsets

When testing sending and receiving data, if your application doesn't terminate
properly the serial port, you may be susceptible to reading data after flushing.
For more details, see the notes in:

`libnserial/NOTES.bug-serialportclose.txt`

The test program to recreate this (raw, without using the library) is given by:

`libnserial/comptest/kernelbug.c`

## Parity

When testing sending data from 8,N,1 to 7,E,1 or 7,O,1 that has a parity error
with parityreplace set to zero, the received bytes appear to be implementation
defined and may affect bytes that are also valid. The same also occurs when
PARMRK is also set, bytes which are not directly affected by the parity error
are marked as so.

Tested were:

* PL2303RA => FAIL
* FTDI     => FAIL
* PL2303H  => FAIL
* 16550A   => PASS

The 16550A chipset is the gold standard, where only the byte that is affected
has its byte set to zero and all other bytes are correct.

The test cases are

* `libnserial/comptest`
  * `SerialParityTest.Parity7O1ReceiveError`
  * `SerialParityTest.Parity7E1ReceiveError`

For example, for the case that the byte 0x45 has received the wrong parity, the
following results might be expected:

In this case, bytes 0x3A to 0x45 were considered incorrect, even though only
bytes 0x45 was the only incorrectly sent byte.

```console
0000030: 3031 3233 3435 3637 3839 ff00 3aff 003b  0123456789..:..;
0000040: ff00 3cff 003d ff00 3eff 003f ff00 40ff  ..<..=..>..?..@.
0000050: 0041 ff00 42ff 0043 ff00 44ff 0045 4647  .A..B..C..D..EFG
```

And in this case 0x44 was considered incorrect.

```console
0000030: 3031 3233 3435 3637 3839 3a3b 3c3d 3e3f  0123456789:;<=>?
0000040: 4041 4243 ff00 4445 4647 4849 4a4b 4c4d  @ABC..DEFGHIJKLM
0000050: 4e4f 5051 5253 5455 5657 5859 5a5b 5c5d  NOPQRSTUVWXYZ[\]
```

## Flow Control

If you close the serial port while blocked (e.g. due to flow control), calling
the serial_close() method may take an excessive amount of time. Times measured
have been from 5 seconds to more than 21 seconds. It appears to be an issue in
the Linux kernel when calling the close() system call.
