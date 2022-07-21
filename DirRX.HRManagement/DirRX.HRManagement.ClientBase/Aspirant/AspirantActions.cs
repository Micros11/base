using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.Aspirant;

namespace DirRX.HRManagement.Client
{
  partial class AspirantActions
  {
    public virtual void StartHiringTask(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.RecruitmentState = Aspirant.RecruitmentState.Approved;
      _obj.Save();
      var task = Functions.Module.Remote.CreateNewHiringTask();
      task.Aspirant = _obj;
      task.Show();
      e.CloseFormAfterAction = true;      
    }

    public virtual bool CanStartHiringTask(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted;
    }

    public virtual void ShowAspirantDocuments(Sungero.Domain.Client.ExecuteActionArgs e)
    {
       Functions.Aspirant.Remote.GetAspirantDocuments(_obj).Show();
    }

    public virtual bool CanShowAspirantDocuments(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted;
    }

    public virtual void CreateReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var report = Functions.Aspirant.Remote.CreateReport(_obj);
      _obj.Report = report;
      _obj.Save();
      if (report.HasVersions)
        report.Open();
      else
        report.Show();
    }

    public virtual bool CanCreateReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Report == null && !_obj.State.IsInserted;
    }

    public virtual void StartRecruitmentTask(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.RecruitmentState = DirRX.HRManagement.Aspirant.RecruitmentState.OnApproval;
      _obj.Save();
      var task = Functions.Module.Remote.CreateNewRecruitmentTask();
      task.AspirantAttachmentGroup.Aspirants.Add(_obj);
      task.Show();
      e.CloseFormAfterAction = true;
    }

    public virtual bool CanStartRecruitmentTask(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted;
    }
  }
}