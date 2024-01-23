#include "pch.h"
#include <iostream>
#include <cstdio>

using namespace System;

namespace Interop {

public ref class MyHelloWorld
{
  public:
    void HelloWorld()
    {
        std::cout << "Hello World" << std::endl;
        char i;
        std::cin >> i;
    }
};

}

