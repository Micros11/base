using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using DirRX.HRManagement.HiringPrepareDocsAssignment;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Server
{
  partial class HiringPrepareDocsAssignmentFunctions
  {
    /// <summary>
    /// Создать документ приложения со списком ЛНА.
    /// </summary>
    /// <returns>Документ приложения.</returns>
    [Remote]
    public IHRDocument CreateLNAList()
    {
      var task = HiringTasks.As(_obj.Task);
      var recordLNA = Functions.Module.GetLNARecord(task.Employee);
      if (recordLNA != null)
      {
        var template = recordLNA.TemplateDocument;
        var docLNA = _obj.ContractAddendum;
        // Если документ не создан ранее, то созать, иначе только перегенерировать
        if (docLNA == null)
        {
          docLNA = HRDocuments.Create();
          docLNA.Subject = HiringPrepareDocsAssignments.Resources.LNADocumentSubject;
          docLNA.Employee = task.Employee;
          docLNA.DocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DocKind.AcquaintanceListKind);
        }
        var body = template.LastVersion.Body.Read();
        // Сформировать новую версию из шаблона
        var newVersion = docLNA.CreateVersionFrom(body, template.AssociatedApplication.Extension);
        // Заполнить макропеременные
        var internalEntity = (Sungero.Domain.Shared.IExtendedEntity)docLNA;
        internalEntity.Params[Sungero.Content.Shared.ElectronicDocumentUtils.FromTemplateIdKey] = template.Id;

        docLNA.Save();
        _obj.ContractAddendum = docLNA;
        return docLNA;
      }
      else
        return null;
    }

    /// <summary>
    /// Проверить, что кадровые документы не сформированы.
    /// </summary>
    /// <returns>True, если если кадровые документы не сформированы.</returns>
    [Remote]
    public bool IsHRDocumentsNotCreated()
    {
      var task = HiringTasks.As(_obj.Task);
      var documentsIsNull = _obj.EmploymentContract == null && _obj.JoiningStatement == null && _obj.Order == null;
      if (task.ExistsProcPersonalDataAgreement.Value && documentsIsNull)
        documentsIsNull = _obj.ProcPersonalDataAgreement == null;
      return documentsIsNull;
    }
    
    /// <summary>
    /// Сформировать кадровые документы сотрудника.
    /// </summary>
    /// <returns>Созданные документы.</returns>
    [Remote]
    public List<IOfficialDocument> CreateEmployeeHRDocuments()
    {
      var task = HiringTasks.As(_obj.Task);
      var employee = task.Employee;
      var result = new List<IOfficialDocument>();
      
      var empContract = Functions.Module.CreateNewHRDocument(DocKind.EmploymentContractKind, employee, task.EmploymentDate.Value);
      result.Add(empContract);
      _obj.EmploymentContract = empContract;
      
      var order = Functions.HiringTask.CreateHiringOrder(task, empContract);
      result.Add(order);
      _obj.Order = order;
      
      var joiningStatement = Functions.Module.CreateNewHRDocument(DocKind.JoiningStatementKind, employee);
      result.Add(joiningStatement);
      _obj.JoiningStatement = joiningStatement;
      
      if (!task.ExistsProcPersonalDataAgreement.Value)
      {
        var procPersonalDataAgreement = Functions.Module.CreateNewHRDocument(DocKind.ProcPersonalDataAgreementKind, employee);
        result.Add(procPersonalDataAgreement);
        _obj.ProcPersonalDataAgreement = procPersonalDataAgreement;
      }
      
      return result;
    }
    
    /// <summary>
    /// Переформировать кадровые документы сотрудника.
    /// </summary>
    [Remote]
    public void RecreateEmployeeHRDocuments()
    {
      var task = HiringTasks.As(_obj.Task);
      Functions.Module.RegenerateDocument(_obj.EmploymentContract);
      Functions.Module.RegenerateDocument(_obj.JoiningStatement);
      Functions.HiringTask.ReCreateHiringOrder(task, _obj.Order);
      
      if (_obj.ProcPersonalDataAgreement != null)
      {
        Functions.Module.RegenerateDocument(_obj.ProcPersonalDataAgreement);
      }
    }
    
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public Sungero.Core.StateView GetHiringPrepareDocsAssignmentState()
    {
      var instruction = string.Empty;
      switch (_obj.BlockUid)
      {
        case Constants.HiringTask.ReworkPersonnelDepBlockId:
          instruction = HiringTasks.Resources.ReWorkDocsAssignmentInstruction;
          break;
        case Constants.HiringTask.PrintDocsBlockId:
          instruction = HiringTasks.Resources.SignAllPaperDocAssignmentInstruction;
          break;
        case Constants.HiringTask.PrintContractDocsBlockId:
          instruction = HiringTasks.Resources.SignPaperEmploymentContractAssignmentInstruction;
          break;
        case Constants.HiringTask.TransferPrepareDocsAssignmentBlockId:
          if (HiringTasks.As(_obj.Task).ExistsProcPersonalDataAgreement.Value)
            instruction = HiringTasks.Resources.HRRegistrationInstruction;
          else
            instruction = HiringTasks.Resources.HRRegistrationNoPersAgreementInstruction;
          break;
      }
      
      return Functions.Module.GetAssignmentStateView(instruction);
    }
  }
}