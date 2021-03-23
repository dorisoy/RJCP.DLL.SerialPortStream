#!/bin/sh
if [ -z "${PROJECTSOURCE}" ]; then
  PROJECTSOURCE="${PWD}"
fi

if [ -z "${PROJECTBIN}" ]; then
  PROJECTBIN="${PWD}/bin"
fi

if [ -e "${PROJECTBIN}" ]; then
  rm -rf "${PROJECTBIN}"/*
fi

if [ -z "${PROJECTBUILD}" ]; then
  PROJECTBUILD="${PWD}/build"
fi

if [ -e "${PROJECTBUILD}" ]; then
  rm -rf "${PROJECTBUILD}"/*
fi

mkdir -p "${PROJECTBUILD}"
cd "${PROJECTBUILD}"

echo ======================================================================
echo == Environment
echo ======================================================================
echo Source Directory: ${PROJECTSOURCE}
echo Build Directory:  ${PROJECTBUILD}
echo Output Directory: ${PROJECTBIN}

# In Ubuntu 14.04, installing 'libgtest-dev' installs the sources and the
# headers, but we have to build ourselves.
if [ -z "${GTEST_ROOT}" ]; then
  if [ -e /usr/src/gtest ]; then
    echo ======================================================================
    echo == Building GTest
    echo ======================================================================
    if [ -e gtest ]; then
      rm -rf ./gtest
    fi
    mkdir gtest
    cd gtest
    cmake /usr/src/gtest && make
    cd ..
    export GTEST_ROOT="${PWD}/gtest"
  fi
fi

echo ======================================================================
echo == Building Project
echo ======================================================================
#Enable this line instead of the other to build with logging
cmake -E env CFLAGS="-O0 -g -Wall" CXXFLAGS="-std=c++11 -Wall" \
  cmake -DCMAKE_BUILD_TYPE=Debug -DNSLOG_ENABLED=OFF "${PROJECTSOURCE}" \
  && make
if test $? = 0; then
  make test CTEST_OUTPUT_ON_FAILURE=1
  make install DESTDIR="${PROJECTBIN}"
fi

SYSTEM=`uname -s`
MACHINE=`uname -m`
if [ -e "${PROJECTBIN}/usr/local/lib/libnserial.so.1.0" ]; then
    cp "${PROJECTBIN}/usr/local/lib/libnserial.so.1.0" "${PROJECTBIN}/usr/local/lib/libnserial.$SYSTEM.$MACHINE.so.1.0"
fi
