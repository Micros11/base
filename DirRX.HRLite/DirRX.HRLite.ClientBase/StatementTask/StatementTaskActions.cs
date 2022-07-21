using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementTask;

namespace DirRX.HRLite.Client
{
  partial class StatementTaskActions
  {
    public override void Start(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var statement = _obj.DocumentGroup.StatementDocuments.FirstOrDefault();
      
        var lastVersion = statement.LastVersion;
        if (lastVersion == null)
        {
          e.AddError(DirRX.HRLite.StatementTasks.Resources.DocumentHasNotVersions);
          return;
        }      
      
      var errorsList = new List<string>();
      var setting = Functions.Module.Remote.GetStatementSetting(statement);
      var needAttachments = Functions.Module.Remote.CheckSettingForStatementAttachments(setting);
      if (needAttachments && _obj.AddendaGroup.InternalDocumentBases.Count == 0)
      {
        e.AddError(Resources.NeedAttachmentsForStatementFormat(statement.DocumentKind.Name));
        return;
      }
      
      var isChanged = statement.History.GetAll().Any(h => h.User == Users.Current && h.Action == Sungero.CoreEntities.History.Action.Update && h.Operation == new Enumeration(Constants.Module.Operation.UpdateVerBody));
      if (!isChanged)
      {
        e.AddError(Resources.CorrectStatementBeforeSending);
        return;
      }
      
      if (Sungero.Docflow.PublicFunctions.Module.VersionIsLocked(statement.Versions.ToList()))
      {
        e.AddError(Resources.SaveAndCloseStatementBeforeSending);
        return;
      }
      
      var message = PublicFunctions.Module.ConvertToPdfa(statement).ErrorMessage;
      var validationErrors = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetApprovalValidationErrors(statement, true);
      if (validationErrors.Any())
      {
        e.AddError(string.Join(Environment.NewLine, validationErrors));
        return;
      }
      
      var statementApproved = Sungero.Docflow.PublicFunctions.Module.ApproveWithAddenda(statement, null, null, null, false, true, string.Empty);
      if (!statementApproved)
        return;
      
      if (!string.IsNullOrEmpty(message))
        errorsList.Add(message);
      if (errorsList.Any())
      {
        e.AddError(string.Join(Environment.NewLine, errorsList));
        return;
      }
      base.Start(e);
    }

    public override bool CanStart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanStart(e);
    }

    public override void Abort(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var abortReason = string.Empty;
      var dialog = Dialogs.CreateInputDialog(Sungero.Docflow.ApprovalTasks.Resources.Confirmation);
      var abortReasonField = dialog.AddMultilineString(StatementTasks.Resources.AbortReason, true);
      dialog.SetOnButtonClick(args =>
                              {
                                if (string.IsNullOrWhiteSpace(abortReasonField.Value))
                                  args.AddError(Sungero.Docflow.ApprovalTasks.Resources.EmptyAbortingReason, abortReasonField);
                              });
      
      if (dialog.Show() == DialogButtons.Ok)
        abortReason = abortReasonField.Value;
      if (abortReason != string.Empty)
      {
        _obj.AbortingReason = abortReason;
        _obj.Save();
        base.Abort(e);
      }
    }

    public override bool CanAbort(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanAbort(e);
    }

  }

}