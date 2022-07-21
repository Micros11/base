using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.RecruitmentTask;

namespace DirRX.HRManagement
{
  partial class RecruitmentTaskSharedHandlers
  {

    public virtual void BusinessUnitChanged(DirRX.HRManagement.Shared.RecruitmentTaskBusinessUnitChangedEventArgs e)
    {
      var businessUnit = e.NewValue;
      if (businessUnit != e.OldValue && _obj.Departments.Count > 0)
        _obj.Departments.Clear();
    }

    public virtual void AspirantAttachmentGroupDeleted(Sungero.Workflow.Interfaces.AttachmentDeletedEventArgs e)
    {
      _obj.ReportAttachmentGroup.AspirantDocuments.Clear();
      _obj.OtherAttachmentGroup.ElectronicDocuments.Clear();
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
    }

    public virtual void AspirantAttachmentGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      // Добавление вложений.
      var aspirant = _obj.AspirantAttachmentGroup.Aspirants.First();
      if (aspirant.Report != null)
        _obj.ReportAttachmentGroup.All.Add(aspirant.Report);
      var otherDocs = Functions.Aspirant.Remote.GetAspirantDocuments(aspirant).Where(n => !Equals(n, aspirant.Report));
      foreach (var otherDoc in otherDocs)
        _obj.OtherAttachmentGroup.ElectronicDocuments.Add(otherDoc);
      _obj.Subject = RecruitmentTasks.Resources.TaskSubjectFormat(aspirant.Name);
    }

  }
}