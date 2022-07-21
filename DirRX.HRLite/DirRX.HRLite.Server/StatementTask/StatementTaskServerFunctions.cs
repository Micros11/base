using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementTask;

namespace DirRX.HRLite.Server
{
  partial class StatementTaskFunctions
  {

    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]      
    public Sungero.Core.StateView StatementInstruction()
    {
      var instruction = string.Empty;
      var statement = _obj.DocumentGroup.StatementDocuments.FirstOrDefault();
      
      var setting = PublicFunctions.Module.Remote.GetStatementSetting(statement);
      var needAttachments = PublicFunctions.Module.Remote.CheckSettingForStatementAttachments(setting);
      instruction = needAttachments ? Resources.StatementTaskInstructionWithAddendum : Resources.StatementTaskInstructionWithoutAddendum;
      return Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}