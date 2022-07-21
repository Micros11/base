using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.PersonelUI.Client
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Сформировать отчет для проверки настроек системы.
    /// </summary>
    public virtual void CreateSettingsReport()
    {
      DirRX.HRManagement.PublicFunctions.Module.CreateSettingsReport();
    }
    
    /// <summary>
    /// Настроить исполнителей ролей по НОР.
    /// </summary>
    public virtual void ConfigureRolesByCompany()
    {
      DirRX.HRManagement.PublicFunctions.Module.ConfigureRolesByCompany();
    }
    
    /// <summary>
    /// Отправить задачу на согласование изменения моих условий труда.
    /// </summary>
    public virtual void StartTransferTaskAsEmployee()
    {
      DirRX.HRManagement.PublicFunctions.Module.StartTransferTaskAsEmployee();
    }
    
    /// <summary>
    /// Создать задачу на увольнение.
    /// </summary>
    public virtual void CreateNewDismissalTask()
    {
      DirRX.HRManagement.PublicFunctions.Module.CreateNewDismissalTask();
    }
    
    /// <summary>
    /// Создать задачу на прием.
    /// </summary>
    public virtual void CreateNewHiringTask()
    {
      DirRX.HRManagement.PublicFunctions.Module.CreateNewHiringTask();
    }
    
    /// <summary>
    /// Отправить задачу на изменения условий труда сотрудника, инициируется руководителем или СОК.
    /// </summary>
    public virtual void StartTransferTaskAsManager()
    {
      DirRX.HRManagement.PublicFunctions.Module.StartTransferTaskAsManager();
    }
    
    /// <summary>
    /// Выгрузить кадровые документы.
    /// </summary>
    public virtual void ExportDocuments()
    {
      DirRX.HRManagement.PublicFunctions.Module.ExportDocuments();
    }
  }
}