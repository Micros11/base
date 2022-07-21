using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using Sungero.Company;
using DirRX.HRManagement.VacationSchedulingTask;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;
using ParamKeys = DirRX.HRManagement.Constants.Module.ParamKey;

namespace DirRX.HRManagement.Server
{
  partial class VacationSchedulingTaskRouteHandlers
  {

    #region Преобразование в Pdf.
    public virtual bool Decision23Result()
    {
      var documents = _obj.VacationScheduleGroup.HRDocuments.ToList();
      var allValid = true;
      if (documents.Any())
      {
        foreach (var document in documents)
        {
          if (document != null && document.Versions.Any())
          {
            var version = document.Versions.LastOrDefault();
            if (!DirRX.HRLite.PublicFunctions.Module.ValidatePdfAFormat(version.Body.Read(), version.AssociatedApplication.Extension))
              allValid = false;
            else
              _obj.ConvertErrorMessage = HRLite.PublicFunctions.Module.ClearConverError(_obj.ConvertErrorMessage, document.Id);
            
          }
        }
      }
      
      return allValid;
    }
    
    public virtual void StartBlock22(DirRX.HRManagement.Server.SchedulingPrintDocAssignmentArguments e)
    {
      e.Block.Performers.Add(_obj.Author);
      e.Block.Subject = VacationSchedulingTasks.Resources.AuthorConvertErrorsReworkSubjectFormat(_obj.VacationScheduleGroup.HRDocuments.FirstOrDefault().Name);
    }

    public virtual bool Decision21Result()
    {
      return string.IsNullOrEmpty(_obj.ConvertErrorMessage);
    }
    
    public virtual void Script20Execute()
    {
      var errorInfoList = new List<string>();
      var doc = _obj.VacationScheduleGroup.HRDocuments.FirstOrDefault();
      var message = HRLite.PublicFunctions.Module.ConvertToPdfa(doc).ErrorMessage;
      message = HRLite.PublicFunctions.Module.GetTextWithLowerCaseFirstSymbol(message);
      if (!string.IsNullOrEmpty(message))
        errorInfoList.Add(DirRX.HRLite.HRAcquaintanceTasks.Resources.DocumentNameWithErrorFormat(doc.Name, doc.Id, message));
      if (errorInfoList.Count > 0)
      {
        var convertErrorMessage = string.Join(Environment.NewLine, errorInfoList);
        _obj.ConvertErrorMessage = convertErrorMessage;
      }
    }
    
    public virtual void Script19Execute()
    {
      // Получить график отпусков в Pdf с подстановкой штампа
      var scheduleReport = _obj.ScheduleReport;
      var versionId = scheduleReport.Versions.Last().Id;
      // Сформировать тело документа с отметкой об ЭП в PDF.
      var documentWithStampFileName = HRLite.PublicFunctions.Module.Remote.GetPdfaDocumentInLocalPdfFileWithStamp(scheduleReport, versionId);
      try
      {
        MemoryStream ms = new MemoryStream();
        using (FileStream file = new FileStream(documentWithStampFileName, FileMode.Open, FileAccess.Read))
        {
          file.CopyTo(ms);
          var version = scheduleReport.Versions.Where(v => v.Id == versionId).FirstOrDefault();
          version.PublicBody.Write(ms);
          version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
          scheduleReport.Save();
        }
        ms.Close();
        if (File.Exists(documentWithStampFileName))
          File.Delete(documentWithStampFileName);
      }
      catch (Exception e)
      {
        Logger.Error(e.Message);
      }
    }
    #endregion

    #region Ознакомление с графиком отпусков
    public virtual bool Decision17Result()
    {
      return Functions.Module.GetModuleSettings(_obj.BusinessUnit).IsVacationAcquaintanceNeeded.Value;
    }
    
    public virtual void Script15Execute()
    {
      var departments = Functions.Module.GetFilteredDepartments(_obj.BusinessUnit);
      var vacationSchedule = _obj.VacationScheduleGroup.HRDocuments.FirstOrDefault();
      foreach (IDepartment department in departments)
      {
        // Если все документы в электронном виде отправить подзадачу на ознакомление, иначе отправить простую задачу.
        if (!Functions.Module.IsAllPaperVacationDoc(_obj.BusinessUnit))
        {
          var acquaintanceTask = HRLite.HRAcquaintanceTasks.CreateAsSubtask(_obj);
          if (acquaintanceTask != null)
          {
            acquaintanceTask.DocumentGroup.InternalDocumentBases.Add(Sungero.Docflow.InternalDocumentBases.As(vacationSchedule));
            acquaintanceTask.Performers.AddNew().Performer = department;
            acquaintanceTask.Signatory = null;
            var acquaintanceDays = Functions.Module.GetModuleSettings(_obj.BusinessUnit).VacationScheduleAcquaintance.Value;
            acquaintanceTask.EmployeeDeadline = acquaintanceDays;
            acquaintanceTask.Author = Users.As(Functions.Module.GetVacationResponsible(_obj.BusinessUnit));
            acquaintanceTask.Start();
          }
        }
        else
        {
          var manager = Functions.Module.GetManager(department);
          if (manager != null)
          {
            var acquaintanceList = Functions.Module.CreateNewVacationAcquaintanceList(department,
                                                                                      _obj.Year.Value.Year,
                                                                                      vacationSchedule.DocumentDate.Value,
                                                                                      vacationSchedule.RegistrationNumber);
            var acquaintanceTask = SimpleTasks.CreateAsSubtask(_obj);
            acquaintanceTask.Attachments.Add(acquaintanceList);
            acquaintanceTask.ActiveText = VacationSchedulingTasks.Resources.SignPaperAcquaintanceList;
            acquaintanceTask.Author = Users.As(Functions.Module.GetVacationResponsible(_obj.BusinessUnit));
            acquaintanceTask.Subject = VacationSchedulingTasks.Resources.SignPaperAcquaintanceListSubjectFormat(department.Name, _obj.Year.Value.Year);
            
            var routeStep = acquaintanceTask.RouteSteps.AddNew();
            routeStep.AssignmentType = Sungero.Workflow.SimpleTask.AssignmentType.Assignment;
            routeStep.Performer = manager;
            acquaintanceTask.Start();
          }
        }
      }
    }
    #endregion
    
    #region Перевод отпусков в состояние "Запланирован" после подписания директором по персоналу
    public virtual void Script14Execute()
    {
      var departments = Sungero.Company.Departments.GetAll(d => d.BusinessUnit.Equals(_obj.BusinessUnit) &&
                                                           d.Status.Equals(Sungero.Company.Department.Status.Active));
      foreach (var department in departments)
      {
        var asyncSetStatus = AsyncHandlers.SetApprovedVacationStatus.Create();
        asyncSetStatus.DepartmentID = department.Id;
        asyncSetStatus.Year = _obj.Year.Value.Year;
        asyncSetStatus.ExecuteAsync();
      }
    }
    #endregion
    
    #region Уведомление ответственному о начале планирования
    public virtual void StartBlock12(DirRX.HRManagement.Server.SchedulingResponsibleNoticeArguments e)
    {
      var perfomer = Functions.Module.GetVacationResponsible(_obj.BusinessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationSchedulingTasks.Resources.SchedulingStartSubjectFormat(_obj.BusinessUnit.Name, _obj.Year.Value.Year);
    }
    #endregion

    #region Согласование графика отпусков в подразделениях
    public virtual void Script3Execute()
    {
      var departments = Functions.Module.GetFilteredDepartments(_obj.BusinessUnit);
      foreach (Sungero.Company.IDepartment department in departments)
      {
        var schedulingSubTask = Functions.Module.CreateNewVacationDepartmentSchedulingTask(_obj.Author);
        if (schedulingSubTask != null)
        {
          Functions.VacationSchedulingTask.FillVacationDepartmentSchedulingTaskFields(_obj, schedulingSubTask, department);
          schedulingSubTask.Start();
        }
      }
    }
    #endregion
    
    #region Мониторинг завершения задач в подразделениях
    public virtual void StartBlock4(Sungero.Workflow.Server.Route.MonitoringStartBlockEventArguments e)
    {
      var monitoringPeriod = Functions.Module.GetDocflowParamValue(ParamKeys.SchedulingMonitoringTimeKey);
      if (string.IsNullOrEmpty(monitoringPeriod))
        e.Block.Period = TimeSpan.FromHours(Constants.VacationSchedulingTask.DefaultMonitoringPeriod);
      else
        e.Block.Period = TimeSpan.FromSeconds(double.Parse(monitoringPeriod));
    }
    
    public virtual bool Monitoring4Result()
    {
      return !VacationDepartmentSchedulingTasks.GetAll(j => Equals(j.MainSchedulingTask, _obj) &&
                                                       j.Status.Value == Sungero.Workflow.Task.Status.InProcess).Any();
    }
    #endregion
    
    #region Согласование и подготовка графика ответственным
    public virtual void StartBlock5(DirRX.HRManagement.Server.SchedulingPrepareDocAssignmentArguments e)
    {
      var perfomer = Functions.Module.GetVacationResponsible(_obj.BusinessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationSchedulingTasks.Resources.CheckVacationScheduleSubjectFormat(_obj.Year.Value.Year, _obj.BusinessUnit.Name);
    }
    
    public virtual void CompleteAssignment5(DirRX.HRManagement.ISchedulingPrepareDocAssignment assignment, DirRX.HRManagement.Server.SchedulingPrepareDocAssignmentArguments e)
    {
      Functions.Module.AddDocflowParam(string.Format(Constants.Module.ParamKey.SchedulingKeyBusinessUnit, _obj.BusinessUnit.Id), false.ToString());
      _obj.ScheduleReport = assignment.VacationSchedule;
    }
    #endregion

    #region Подписание Директором по персоналу
    public virtual void StartBlock11(DirRX.HRManagement.Server.SchedulingSignDocAssignmentArguments e)
    {
      var perfomer = Functions.Module.GetStaffChief(_obj.BusinessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationSchedulingTasks.Resources.SignSeoAssignmentSubjectFormat(_obj.Year.Value.Year, _obj.BusinessUnit.Name);
    }

    public virtual void CompleteAssignment11(DirRX.HRManagement.ISchedulingSignDocAssignment assignment, DirRX.HRManagement.Server.SchedulingSignDocAssignmentArguments e)
    {
      var scheduleReport = _obj.ScheduleReport;
      scheduleReport.LifeCycleState = HRManagement.HRDocument.LifeCycleState.Active;
      scheduleReport.Save();
    }
    #endregion
    
    #region Все документы в бумажном виде?
    public virtual bool Decision7Result()
    {
      return Functions.Module.IsAllPaperVacationDoc(_obj.BusinessUnit);
    }
    #endregion
    
    #region Печать документов ответственным за график отпусков
    public virtual void StartBlock8(DirRX.HRManagement.Server.SchedulingPrintDocAssignmentArguments e)
    {
      var perfomer = Functions.Module.GetVacationResponsible(_obj.BusinessUnit);
      e.Block.Performers.Add(perfomer);
      e.Block.Subject = VacationSchedulingTasks.Resources.SignAllPaperDocSubject;
    }
    #endregion

  }
}