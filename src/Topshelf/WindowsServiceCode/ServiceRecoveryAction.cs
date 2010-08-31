using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Topshelf.WindowsServiceCode
{
  public enum ServiceRecoveryAction
  {
    TakeNoAction,
    RestartTheService,
    RunAProgram,
    RestartTheComputer
  }
}
