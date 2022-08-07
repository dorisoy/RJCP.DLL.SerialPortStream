# README for Cygwin

This package will compile on Cygwin.

## Running Unit Tests

To run unit tests, you should download the GoogleTest framework from github,
compile and install.

```sh
$ git clone https://github.com/google/googletest.git
$ mkdir gtestbuild
$ cd gtestbuild
$ cmake ../googletest
$ make
$ make install
```

Then you can build the serialunix package and it will build the unit test cases.
The next problem you'll come across on Cygwin is running the unit and component
tests. It will initially fail that it can't find the library that is built. To
overcome this problem, run the tests manually (replace SERIALUNIXBASE with the
path to the serialunix package).

```sh
$ export SERIALUNIXBASE=/home/user/serialunix
$ export PATH=$PATH:$SERIALUNIXBASE/build/libnserial
$ cd $SERIALUNIXBASE/build/libnserial/unittest
$ ./nserialtest.exe
$ cd $SERIALUNIXBASE/build/libnserial/comptest
$ ./nserialcomptest.exe
```

## Parity Unit Tests

Under Cygwin32:

`CYGWIN_NT-6.1 leon 2.4.1(0.293/5/3) 2016-01-24 11:24 i686 Cygwin`

The parity unit test cases fail. It appears that under Cygwin there is no parity
checking at all. The same test cases appear to work correctly with Linux using a
16550A UART.

So the unit test cases:

* `SerialParityTest.Parity7E1ReceiveError`
* `SerialParityTest.Parity7O1ReceiveError`
* `SerialParityTest.Parity7O1ReceiveError`

appear to fail.