using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HiringPrepareDocsAssignment;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Client
{
  partial class HiringPrepareDocsAssignmentActions
  {

    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid == Constants.HiringTask.ReworkPersonnelDepBlockId;
    }

    public virtual void GenerateEmployeeDocuments(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var warningMessage = string.Empty;
      if (Functions.HiringPrepareDocsAssignment.Remote.IsHRDocumentsNotCreated(_obj))
      {
        var docs = Functions.HiringPrepareDocsAssignment.Remote.CreateEmployeeHRDocuments(_obj);
        // Сформировать приложения к трудовому договору.
        var docWithLNAList = Functions.HiringPrepareDocsAssignment.Remote.CreateLNAList(_obj);
        if (docWithLNAList != null)
        {
          docs.Add(docWithLNAList);
          docWithLNAList.EmploymentContract = _obj.EmploymentContract;
          docWithLNAList.Save();
        }
        else
          warningMessage = HiringPrepareDocsAssignments.Resources.LNADocumentNotCreated;
        foreach (var doc in docs)
          _obj.HRDocumentAttachmentGroup.All.Add(doc);
        _obj.Save();
      }
      else
      {
        // Переформировать документы.
        Functions.HiringPrepareDocsAssignment.Remote.RecreateEmployeeHRDocuments(_obj);
        var docWithLNAList = Functions.HiringPrepareDocsAssignment.Remote.CreateLNAList(_obj);
        if (docWithLNAList == null)
          warningMessage = HiringPrepareDocsAssignments.Resources.LNADocumentNotCreated;
        Dialogs.NotifyMessage(HiringPrepareDocsAssignments.Resources.DocumentsReform);
      }
      if (!string.IsNullOrEmpty(warningMessage))
        e.AddWarning(warningMessage);
    }

    public virtual bool CanGenerateEmployeeDocuments(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Sungero.Workflow.AssignmentBase.Status.InProcess && _obj.BlockUid == Constants.HiringTask.TransferPrepareDocsAssignmentBlockId;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.BlockUid != Constants.HiringTask.ReworkPersonnelDepBlockId;
    }

  }

}