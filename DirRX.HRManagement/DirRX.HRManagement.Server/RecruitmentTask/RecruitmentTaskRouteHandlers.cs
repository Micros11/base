using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.HRManagement.RecruitmentTask;

namespace DirRX.HRManagement.Server
{
  partial class RecruitmentTaskRouteHandlers
  {

    public virtual void StartBlock3(DirRX.HRManagement.Server.RecruitmentAssignmentArguments e)
    {
      List<Sungero.Company.IDepartment> departments = _obj.Departments.Select(d => d.Department).ToList();
      foreach (var department in departments)
        e.Block.Performers.Add(department.Manager);
      e.Block.Subject = RecruitmentTasks.Resources.ManagerAssignmentSubjectFormat(_obj.Aspirant.Name);
    }
    
    public virtual void StartBlock4(DirRX.HRManagement.Server.HRRecruitmentAssignmentArguments e)
    {
      var perfomer = Functions.Module.GetHRResponsible(_obj.BusinessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = RecruitmentTasks.Resources.PlanMeetingHRAssignmentSubjectFormat(_obj.Aspirant.Name);
    }
    
    public virtual void CompleteAssignment4(DirRX.HRManagement.IHRRecruitmentAssignment assignment, DirRX.HRManagement.Server.HRRecruitmentAssignmentArguments e)
    {
      if (assignment.Result == DirRX.HRManagement.HRRecruitmentAssignment.Result.Finish)
        Functions.Module.SendAbortationNotices(_obj, RecruitmentTasks.Resources.AbortNoticeFormat(_obj.Aspirant.Name), assignment.ActiveText, assignment.Performer, _obj.ThreadSubject);
    }
    
    public virtual void StartBlock5(DirRX.HRManagement.Server.RecruitmentAssignmentArguments e)
    {
      var meetingAssignment = HRRecruitmentAssignments.GetAll(a => Equals(a.Task, _obj) && a.BlockUid == Constants.RecruitmentTask.HRMeetingBlockId && a.TaskStartId == a.Task.StartId).FirstOrDefault();
      if (meetingAssignment.Managers.Any())
      {
        List<Sungero.Company.IEmployee> managers = meetingAssignment.Managers.Select(m => m.Employee).ToList();
        foreach (var manager in managers)
          e.Block.Performers.Add(manager);
        e.Block.Subject = RecruitmentTasks.Resources.ManagerReportAssignmentSubjectFormat(_obj.Aspirant.Name);
      }
    }
    
    public virtual void StartBlock6(DirRX.HRManagement.Server.RecruitmentAssignmentArguments e)
    {
      var perfomer = Functions.Module.GetStaffChief(_obj.BusinessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = RecruitmentTasks.Resources.SeoReportAssignmentSubjectFormat(_obj.Aspirant.Name);
    }
    
    public virtual void StartBlock7(DirRX.HRManagement.Server.HRRecruitmentAssignmentArguments e)
    {
      var perfomer = Functions.Module.GetHRResponsible(_obj.BusinessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = RecruitmentTasks.Resources.PlanSecurityAssignmentSubjectFormat(_obj.Aspirant.Name);
    }

    public virtual void CompleteAssignment7(DirRX.HRManagement.IHRRecruitmentAssignment assignment, DirRX.HRManagement.Server.HRRecruitmentAssignmentArguments e)
    {
      if (assignment.Result == DirRX.HRManagement.HRRecruitmentAssignment.Result.Finish)
        Functions.Module.SendAbortationNotices(_obj, RecruitmentTasks.Resources.AbortNoticeFormat(_obj.Aspirant.Name), assignment.ActiveText, assignment.Performer, _obj.ThreadSubject);
    }
    
    public virtual void StartBlock9(DirRX.HRManagement.Server.RecruitmentAssignmentArguments e)
    {
      var perfomer = Functions.Module.GetSecurityResponsible(_obj.BusinessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = RecruitmentTasks.Resources.SecurityAssignmentSubjectFormat(_obj.Aspirant.Name);
    }

    public virtual void StartBlock10(DirRX.HRManagement.Server.HRRecruitmentAssignmentArguments e)
    {
      var perfomer = Functions.Module.GetHRResponsible(_obj.BusinessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = RecruitmentTasks.Resources.PrepareHiringAssignmentSubjectFormat(_obj.Aspirant.Name);
    }
    
    public virtual void CompleteAssignment10(DirRX.HRManagement.IHRRecruitmentAssignment assignment, DirRX.HRManagement.Server.HRRecruitmentAssignmentArguments e)
    {
      if (assignment.Result == DirRX.HRManagement.HRRecruitmentAssignment.Result.Finish)
        Functions.Module.SendAbortationNotices(_obj, RecruitmentTasks.Resources.AbortNoticeFormat(_obj.Aspirant.Name), assignment.ActiveText, assignment.Performer, _obj.ThreadSubject);
    }
    
    public virtual void StartBlock14(DirRX.HRManagement.Server.HRRecruitmentAssignmentArguments e)
    {
      var perfomer = Functions.Module.GetHRResponsible(_obj.BusinessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = RecruitmentTasks.Resources.PlanSeoAssignmentSubjectFormat(_obj.Aspirant.Name);
    }
  }

}