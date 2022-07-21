using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRManagement.TransferPrepareDocsAssignment;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Server
{
  partial class TransferPrepareDocsAssignmentFunctions
  {

    /// <summary>
    /// Проверить, что доп соглашение и приказ не сформированы.
    /// </summary>
    /// <returns>True, если доп соглашение и приказ не сформированы.</returns>
    [Remote]
    public bool IsHRDocumentsNotCreated()
    {
      return _obj.AdditionalAgreement == null && _obj.Order == null;
    }
    
    /// <summary>
    /// Переформировать доп соглашение и приказ.
    /// </summary>
    [Remote]
    public void ReCreateEmployeeHRDocuments()
    {
      var task = TransferTasks.As(_obj.Task);
      Functions.Module.RegenerateDocument(_obj.AdditionalAgreement);
      Functions.TransferTask.ReCreateTransferOrder(task, _obj.Order);
    }
    
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetTransferPrepareDocsAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.TransferTask.TransferPrepareDocsAssignmentBlockId:
          var task = TransferTasks.As(_obj.Task);
          if (task.IsHRStarted.Value || TransferPrepareDocsAssignments.GetAll(a => Equals(a.Task, task) && a.BlockUid == Constants.TransferTask.TransferPrepareDocsAssignmentBlockId && a.Id != _obj.Id).Any())
            instruction = TransferTasks.Resources.RePrepareDocAssignmentInstruction;
          else
            instruction = TransferTasks.Resources.CheckDocAssignmentInstruction;
          break;
        case Constants.TransferTask.ReworkTransferPrepareDocsAssignmentBlockId:
          instruction = TransferTasks.Resources.ReWorkDocsAssignmentInstruction;
          break;
        case Constants.TransferTask.PrintBlockId:
          instruction = TransferTasks.Resources.SignAllPaperDocAssignmentInstruction;
          break;
        case Constants.TransferTask.PrintAddAgreementBlockId:
          instruction = TransferTasks.Resources.SignPaperAddAgreementAssignmentInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }
  }
}