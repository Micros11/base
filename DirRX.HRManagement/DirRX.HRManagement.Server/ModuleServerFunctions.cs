using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Company;
using Sungero.Content;
using Sungero.Domain.Shared;
using Sungero.Domain.LinqExpressions;
using Sungero.Workflow;
using Aspose.Cells;
using Newtonsoft.Json;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;
using DocKindFunctions = Sungero.Docflow.PublicFunctions.DocumentKind;
using HRRoles = DirRX.HRManagement.Constants.Module.HRRoles;
using OfficialDocument = Sungero.Docflow.OfficialDocument;
using ParamKeys = DirRX.HRManagement.Constants.Module.ParamKey;

namespace DirRX.HRManagement.Server
{
  public class ModuleFunctions
  {

    #region Лицензирование.
    
    /// <summary>
    /// Проверить наличие лицензии на модуль Отпуска.
    /// </summary>
    [Public, Remote]
    public void CheckVacationsModuleLicense()
    {
      if (!PublicFunctions.Module.Remote.CheckModuleLicense(DirRX.VacationsUI.PublicConstants.Module.ModuleGuid, false))
      {
        Logger.Error(Resources.NoLicenseVacations);
        throw new AppliedCodeException(Resources.NoLicenseVacations);
      }
    }
    
    /// <summary>
    /// Проверить наличие лицензии на модуль Движение персонала.
    /// </summary>
    [Public, Remote]
    public void CheckPersonnelModuleLicense()
    {
      if (!PublicFunctions.Module.Remote.CheckModuleLicense(DirRX.PersonelUI.PublicConstants.Module.ModuleGuid, false))
      {
        Logger.Error(Resources.NoLicensePersonnel);
        throw new AppliedCodeException(Resources.NoLicensePersonnel);
      }
    }

    /// <summary>
    /// Проверить наличие лицензии на модуль.
    /// </summary>
    /// <param name="moduleGuid">Guid модуля.</param>
    /// <param name="checkKeys">Проверить наличие ключа(ей) лицензии.</param>
    /// <returns>Признак наличия лицензии на модуль.</returns>
    [Public, Remote]
    public bool CheckModuleLicense(Guid moduleGuid, bool checkKeys)
    {
      var hasLicense = Sungero.Docflow.PublicFunctions.Module.Remote.IsModuleAvailableByLicense(moduleGuid);
      if (checkKeys == true)
      {
        Dictionary<int, byte[]> licenses = null;
        licenses = Sungero.Docflow.PublicFunctions.Module.ReadLicense();
        hasLicense = hasLicense && licenses.Count > 0;
      }
      
      return hasLicense;
    }
    
    #endregion
    
    #region Настройка выбора подписи.
    
    /// <summary>
    /// Проверить, требуются ли оригиналы бумажных документов.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>True, если требуются оригиналы бумажных документов.</returns>
    public bool IsPaperDocumentsRequied(IEmployee employee)
    {
      return this.IsAllPaperStaffDoc(Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee)) || !this.IsEmployeeUsingESS(employee);
    }
    
    /// <summary>
    /// Вычислить, работает ли сотрудник в Личном кабинете.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>
    /// True, если сотрудник подключен к ЛК, иначе false.
    /// </returns>
    [Remote, Public]
    public bool IsEmployeeUsingESS(IEmployee employee)
    {
      return DirRX.EssPlatformSolution.Employees.As(employee).PersonalAccountStatusDirRX == DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted;
    }

    /// <summary>
    /// Проверить требуется ли от Директора по персоналу усиленная подпись.
    /// </summary>
    /// <param name="staffChief">Директор по персоналу.</param>
    /// <returns>True - если требуется усиленная подпись.</returns>
    public bool NeedStaffChiefAdvancedSign(IEmployee staffChief)
    {
      if (Functions.Module.HasValidCertificate(staffChief))
        return true;
      
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(staffChief);
      var moduleSettings = Functions.Module.GetModuleSettings(businessUnit);
      
      if (moduleSettings.OriginalDocInStaff.Value == HRManagement.HRSettings.OriginalDocInStaff.AllPaperDoc &&
          moduleSettings.OriginalDocInVacation.Value == HRManagement.HRSettings.OriginalDocInVacation.AllPaperDoc)
        return false;
      return true;
    }

    /// <summary>
    /// Проверить требуется ли от сотрудника усиленная подпись.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="blockUid">ИД блока задачи.</param>
    /// <param name="staffChiefBlockId">Список блоков, для которых требуется подпись Директора по персоналу.</param>
    /// <param name="employeeBlockId">Список блоков, для которых требуется подпись Сотрудника.</param>
    /// <returns> True, если BlockUid в списке staffChiefBlockId и требуется усиленная подпись Директора по персоналу
    /// или если BlockUid в списке employeeBlockId и требуется усиленная подпись сотрудника.
    /// </returns>
    [Remote]
    public bool NeedAdvancedSign(IEmployee employee, string blockUid,  List<string> staffChiefBlockId, List<string> employeeBlockId)
    {
      // HACK Если ЛК подключен к системе, то ПЭП использовать нельзя и вся система работает в режиме "требуется УНЭП". Ограничение для версии 1.3.
      if (DirRX.EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
        return true;
      
      if (staffChiefBlockId.Contains(blockUid))
        return this.NeedStaffChiefAdvancedSign(employee);
      
      var needAdvancedSignForBusinessUnit = this.IsQualifiedSignature(employee);
      if (needAdvancedSignForBusinessUnit)
        if (employeeBlockId.Contains(blockUid))
          return true;
      
      return false;
    }
    
    /// <summary>
    /// Вычислить использует ли усиленную подпись указанный сотрудник.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>True, если сотрудник будет подписывать усиленной подписью, false если ПЭП.</returns>
    [Remote, Public]
    public bool IsQualifiedSignature(IEmployee employee)
    {
      var employeeO = DirRX.HRSolution.Employees.As(employee);
      // Если вид подписи для сотрудника указан как усиленная подпись - будет подписывать указанным видом.
      if (employeeO.SignKindDirRX == DirRX.HRSolution.Employee.SignKindDirRX.Quailified)
        return true;
      // Если наследовать от подразделения.
      else
      {
        var departmentO = DirRX.HRSolution.Departments.As(employee.Department);
        // Если для подразделения явно указана ПЭП, то подписывать будут ПЭП.
        if (departmentO.SignKindDirRX == DirRX.HRSolution.Department.SignKindDirRX.Simple)
          return false;
        // Если вид подписи для подразделения явно указан как усиленная подпись - будeт подписывать усиленной подписью.
        else if (departmentO.SignKindDirRX == DirRX.HRSolution.Department.SignKindDirRX.Qualified)
          return true;
        // Если наследовать от НОР - взять свойство Требовать у сотрудников усиленную подпись справочника Настройки модуля соответствующей НОР.
        else
        {
          var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
          if (businessUnit != null)
          {
            var needAdvancedSign = Functions.Module.GetModuleSettings(businessUnit).NeedAdvancedSign;
            if (needAdvancedSign != null)
              return needAdvancedSign.Value;
          }
          return false;
        }
      }
    }
    #endregion
    
    #region Выгрузка документов.
    
    /// <summary>
    /// Найти кадровые документы по заданным параметрам.
    /// </summary>
    /// <param name="isVacationDocuments">True, если искать документы по отпускам, False для прочих кадровых.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <param name="department">Подразделение.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="documentKinds">Виды документов.</param>
    /// <param name="beginPeriod">Дата с.</param>
    /// <param name="endPeriod">Дата по.</param>
    /// <returns>Список документов для выгрузки.</returns>
    [Remote, Public]
    public IQueryable<Sungero.Docflow.IOfficialDocument> SearchByRequisites(bool isVacationDocuments, IBusinessUnit businessUnit, IDepartment department, IEmployee employee,
                                                                            List<IDocumentKind> documentKinds, DateTime? beginPeriod, DateTime? endPeriod)
    {
      IQueryable<Sungero.Docflow.IOfficialDocument> documents = null;
      // Отфильтровать по НОР и видам документов: если в параметре documentKinds виды указаны, то взять их, если нет - взять все виды кадровых/отпускных документов
      if (!documentKinds.Any())
        documentKinds = GetHRDocumentKinds(isVacationDocuments);
      documents = OfficialDocuments.GetAll(d => d.BusinessUnit.Equals(businessUnit) && documentKinds.Contains(d.DocumentKind));
      
      // По подраздедению или сотруднику
      if (employee != null)
        documents = documents.Where(d => HRDocuments.As(d).Employee.Equals(employee) || HROrders.As(d).Employee.Equals(employee));
      else if (department != null)
      {
        // В документах, касающихся перевода сотрудника, смотрим и новое, и старое подразделение
        documents = documents.Where(d => d.Department.Equals(department) ||
                                    (HRDocuments.Is(d) && HRDocuments.As(d).NewDepartment.Equals(department)) ||
                                    (HROrders.Is(d) && HROrders.As(d).NewDepartment.Equals(department)));
      }
      
      // По периоду
      if (isVacationDocuments)
      {
        // В документах по отпускам сравнивать по учетному году, свойство зависит от типа документа
        if (beginPeriod != null)
          documents = documents.Where(d => (HRDocuments.Is(d) && HRDocuments.As(d).Year.Value.Year >= beginPeriod.Value.Year) ||
                                      (HROrders.Is(d) && HROrders.As(d).Year.Value.Year >= beginPeriod.Value.Year));
        if (endPeriod != null)
          documents = documents.Where(d => (HRDocuments.Is(d) && HRDocuments.As(d).Year.Value.Year <= endPeriod.Value.Year) ||
                                      (HROrders.Is(d) && HROrders.As(d).Year.Value.Year <= endPeriod.Value.Year));
      }
      else
      {
        // В кадровых документах взять полную дату, дата для сравнения с периодом зависит от типа и вида документа:
        // Для приказов - EventDate
        // Для заявлений по движениям персонала, трудового договора и допсоглашения - дата соответствующего события
        if (beginPeriod != null)
          documents = documents.Where(d => (HROrders.Is(d) && HROrders.As(d).EventDate.Value >= beginPeriod.Value) ||
                                      (Equals(d.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.EmploymentContractKind)) && HRDocuments.As(d).WorkDate >= beginPeriod.Value) ||
                                      (Equals(d.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.TransferAdditionalAgreementKind)) && HRDocuments.As(d).TransferDate >= beginPeriod.Value) ||
                                      (Equals(d.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.ChangeWorkConditionsAdditionalAgreementKind)) && HRDocuments.As(d).TransferDate >= beginPeriod.Value) ||
                                      (Equals(d.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.TransferStatementKind)) && HRDocuments.As(d).TransferDate >= beginPeriod.Value) ||
                                      (Equals(d.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.DismissalStatementKind)) && HRDocuments.As(d).DismissalDate >= beginPeriod.Value));
        if (endPeriod != null)
          documents = documents.Where(d => (HROrders.Is(d) && HROrders.As(d).EventDate.Value <= endPeriod.Value) ||
                                      (Equals(d.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.EmploymentContractKind)) && HRDocuments.As(d).WorkDate <= endPeriod.Value) ||
                                      (Equals(d.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.TransferAdditionalAgreementKind)) && HRDocuments.As(d).TransferDate <= endPeriod.Value) ||
                                      (Equals(d.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.ChangeWorkConditionsAdditionalAgreementKind)) && HRDocuments.As(d).TransferDate <= endPeriod.Value) ||
                                      (Equals(d.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.TransferStatementKind)) && HRDocuments.As(d).TransferDate <= endPeriod.Value) ||
                                      (Equals(d.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.DismissalStatementKind)) && HRDocuments.As(d).DismissalDate <= endPeriod.Value));
      }
      
      // Проверить наличие подписей на документах в зависимости от вида
      // Преобразование типов сделано, потому что внутри IQueryable работа с Signatures не поддерживается
      documents = documents.ToList().Where(d => this.NeedExportDocWithSignature(d)).AsQueryable();
      
      return documents;
    }

    /// <summary>
    /// Проверить наличие подписей, необходимых для выгрузки документа.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>True, если документ нужно выгружать.</returns>
    public virtual bool NeedExportDocWithSignature(IOfficialDocument document)
    {
      var isPaperCopies = !this.NeedConvertToPdf(document);
      if (isPaperCopies)
      {
        var version = document.LastVersion;
        if (version != null && this.IsApplicationForScanCopies(version.AssociatedApplication))
          return true;
        else
          return false;
      }
      
      // Уведомление о начале отпуска выгружать без учета подписей
      return (document.DocumentKind == DocKindFunctions.GetNativeDocumentKind(DocKind.ScheduledVacationNoticeKind)) ||
        // График отпусков с подписью Директора по персоналу - подписывающего
        (document.DocumentKind == DocKindFunctions.GetNativeDocumentKind(DocKind.VacationScheduleKind) && GetHRManagerSignature(document, !isPaperCopies) != null) ||
        // Приказы с подписью хотя бы одной из сторон - подписывающего или сотрудника
        (HROrders.Is(document) && (GetHRManagerSignature(document, !isPaperCopies) != null || GetEmployeeSignature(document, !isPaperCopies) != null)) ||
        // Заявления (исключая уведомление о начале отпуска и график отпусков) с подписью сотрудника
        (HRDocuments.Is(document) &&
         document.DocumentKind != DocKindFunctions.GetNativeDocumentKind(DocKind.ScheduledVacationNoticeKind) &&
         document.DocumentKind != DocKindFunctions.GetNativeDocumentKind(DocKind.VacationScheduleKind) &&
         GetEmployeeSignature(document, !isPaperCopies) != null) ||
        // Трудовой договор и допсоглашение с подписью хотя бы одной из сторон
        (HRDocuments.Is(document) &&
         (document.DocumentKind == DocKindFunctions.GetNativeDocumentKind(DocKind.EmploymentContractKind) ||
          document.DocumentKind == DocKindFunctions.GetNativeDocumentKind(DocKind.TransferAdditionalAgreementKind) ||
          document.DocumentKind == DocKindFunctions.GetNativeDocumentKind(DocKind.ChangeWorkConditionsAdditionalAgreementKind)) &&
         (GetEmployeeSignature(document, isPaperCopies) != null || GetHRManagerSignature(document, isPaperCopies) != null));
    }
    
    /// <summary>
    /// Проверить, является ли приложение-обработчик допустимым для скан-копий.
    /// </summary>
    /// <param name="app">Приложение.</param>
    /// <returns>True, если приложение-обработчик возможно для скан копий.</returns>
    public virtual bool IsApplicationForScanCopies(IAssociatedApplication app)
    {
      return app == AssociatedApplications.GetByExtension("pdf");
    }
    
    /// <summary>
    /// Получить список видов документов решения HR-процессы, которые необходимо выгружать.
    /// </summary>
    /// <param name="isVacationDocuments">True, если выгружать будем документы по отпускам, False для прочих кадровых.</param>
    /// <returns>Список видов кадровых документов.</returns>
    [Remote, Public]
    public static List<IDocumentKind> GetHRDocumentKinds(bool isVacationDocuments)
    {
      var documentKinds = DocumentKinds.GetAll(k => k.DocumentType.DocumentTypeGuid == HRDocument.ClassTypeGuid.ToString() ||
                                               k.DocumentType.DocumentTypeGuid == HROrder.ClassTypeGuid.ToString());
      // По видам документов: кадровые или по отпускам
      if (isVacationDocuments)
      {
        documentKinds = documentKinds.Where(k => Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.VacationScheduleKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.VacationStatementKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.VacationOrderKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.VacationShiftStatementKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.VacationShiftOrderKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.VacationRecallStatementKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.VacationRecallOrderKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.ScheduledVacationNoticeKind)));
      }
      else
      {
        documentKinds = documentKinds.Where(k => Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.EmploymentContractKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.HiringOrderKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.TransferAdditionalAgreementKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.ChangeWorkConditionsAdditionalAgreementKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.TransferStatementKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.TransferOrderKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.ChangeWorkConditionsOrderKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.DismissalStatementKind)) ||
                                            Equals(k, DocKindFunctions.GetNativeDocumentKind(DocKind.DismissalOrderKind)));
      }
      return documentKinds.OrderBy(k => k.Name).ToList();
    }
    
    /// <summary>
    /// Подготовить кадровые документы для выгрузки.
    /// </summary>
    /// <param name="documents">Документы для выгрузки.</param>
    /// <param name="forPrint">Формат выгрузки: для печати или в электронном виде. True - для печати.</param>
    /// <returns>Список описаний документов.</returns>
    [Remote]
    public List<Structures.Module.ExportedDocument> PrepareExportHRDocuments(List<IOfficialDocument> documents, bool forPrint)
    {
      var result = new List<Structures.Module.ExportedDocument>();
      foreach (var document in documents)
      {
        var needConvert = this.NeedConvertToPdf(document);
        var isFullComplect = this.IsFullComplectInDocument(document, needConvert);
        var docStructure = Structures.Module.ExportedDocument
          .Create(document.Id, false, false, forPrint, string.Empty, Structures.Module.ExportedFolder
                  .Create(string.Empty, new List<Structures.Module.ExportedFolder>(), string.Empty), document.Name, null, isFullComplect, needConvert);
        
        result.Add(docStructure);

        Logger.DebugFormat("Document with id '{0}' has been prepared for export documents. For print: '{1}'.", document.Id,  forPrint);
        
        // Если документ в Pdf не нужно преобразовывать, значит для этих документов требуются оригиналы бумажных документов.
        // Документы разделить по папкам, взависимости от наличия полного комплекта подписей на них и требования оригиналов бумажных дкоументов.
        var folder = docStructure.Folder;
        var folderName = string.Empty;
        if (!needConvert)
          folderName = CommonLibrary.FileUtils.NormalizeFileName(Resources.CopiesOfPaperDocFolderName);
        else
          folderName = CommonLibrary.FileUtils.NormalizeFileName(isFullComplect ? Resources.FullComplectSignatureFolderName : Resources.NoFullComplectSignatureFolderName);
        
        var subfolder = Structures.Module.ExportedFolder
          .Create(folderName, new List<Structures.Module.ExportedFolder>(), folder.FolderName);
        folder.Folders.Add(subfolder);
        folder = subfolder;
        
        // Добавить приложения.
        foreach (var addendumDocument in document.Relations.GetRelated(Sungero.Docflow.PublicConstants.Module.AddendumRelationName).ToList())
        {
          var structure = Structures.Module.ExportedDocument.Create(addendumDocument.Id, true,
                                                                    false, forPrint, string.Empty,
                                                                    docStructure.Folder, addendumDocument.Name, document.Id, isFullComplect, needConvert);
          result.Add(structure);
          
          Logger.DebugFormat("Addendum with id '{0}' has been prepared for export documents. For print: '{1}', lead document id '{2}'.",
                             addendumDocument.Id, forPrint, document.Id);
        }
        
        // Обработать  листы ознакомления с ЛНА, выгружать их как приложения к трудовому договору.
        if (GetDocumentKindGuid(document.DocumentKind) == DocKind.EmploymentContractKind)
        {
          var acquaintanceList = HRDocuments.GetAll(d => d.DocumentKind == DocKindFunctions.GetNativeDocumentKind(DocKind.AcquaintanceListKind) &&
                                                    d.EmploymentContract.Id == document.Id).FirstOrDefault();
          if (acquaintanceList != null)
          {
            var structure = Structures.Module.ExportedDocument.Create(acquaintanceList.Id, true,
                                                                      false, forPrint, string.Empty,
                                                                      docStructure.Folder, acquaintanceList.Name, document.Id, isFullComplect, needConvert);
            result.Add(structure);
            
            Logger.DebugFormat("Addendum with id '{0}' has been prepared for export documents. For print: '{1}', lead document id '{2}'.",
                               acquaintanceList.Id, forPrint, document.Id);
          }
        }
      }
      return result;
    }
    
    /// <summary>
    /// Получить имя документа для выгрузки.
    /// </summary>
    /// <param name="expDocument">Описание документа.</param>
    /// <returns>Имя документа.</returns>
    private static string GetExportedDocumentFileName(Structures.Module.ExportedDocument expDocument)
    {
      var name = expDocument.Name;
      if (expDocument.IsPrint)
        name = name + " (" + expDocument.Id + ")";
      
      return CommonLibrary.FileUtils.NormalizeFileName(name);
    }
    
    /// <summary>
    /// Сформировать папку документа. Для приложений и документов для печати подпапки не создавать.
    /// </summary>
    /// <param name="expDocument">Описание документа.</param>
    /// <returns>Папка для документа.</returns>
    private static Structures.Module.ExportedFolder GetRealDocumentFolder(Structures.Module.ExportedDocument expDocument)
    {
      var innerFolder = expDocument.Folder;
      while (innerFolder.Folders != null && innerFolder.Folders.Any())
        innerFolder = innerFolder.Folders.Single();
      
      if (!expDocument.IsPrint && !expDocument.IsAddendum && expDocument.NeedConvert)
      {
        var folderName = CommonLibrary.FileUtils.NormalizeFileName(expDocument.Name) + " (" + expDocument.Id + ")";
        if (folderName == innerFolder.FolderName)
          return innerFolder;
        
        var subFolder = Structures.Module.ExportedFolder
          .Create(folderName, new List<Structures.Module.ExportedFolder>(), innerFolder.FolderName);
        innerFolder.Folders.Add(subFolder);
        return subFolder;
      }
      return innerFolder;
    }
    
    /// <summary>
    /// Подготовить тела документов и подписи для выгрузки.
    /// </summary>
    /// <param name="expDocuments">Описание документов.</param>
    /// <returns>Результаты выгрузки.</returns>
    [Remote]
    public Structures.Module.ExportResult PrepareBodiesAndSignsDocuments(List<Structures.Module.ExportedDocument> expDocuments)
    {
      var result = Structures.Module.ExportResult.Create();
      var zipModels = new List<Structures.Module.ZipModel>();
      foreach (var document in expDocuments)
      {
        try
        {
          // Для приложений проверить, что выгружен основной документ.
          if (document.IsAddendum)
          {
            var leadingDocument = expDocuments.Where(x => Equals(x.Id, document.LeadDocumentId)).FirstOrDefault();
            if (leadingDocument != null && leadingDocument.IsFaulted == true)
            {
              document.IsFaulted = true;
              document.Error = Resources.ExportDialog_Error_NoVersion;
              continue;
            }
          }
          
          // Проверить, что есть версия документа.
          var doc = OfficialDocuments.Get(document.Id);
          if (!doc.HasVersions)
          {
            document.IsFaulted = true;
            document.Error = Resources.ExportDialog_Error_NoVersion;
            continue;
          }
          
          var version = doc.LastVersion;
          var fileName = GetExportedDocumentFileName(document);
          var folder = GetRealDocumentFolder(document);
          if (document.IsAddendum)
            folder = GetRealDocumentFolder(expDocuments.Where(x => Equals(x.Id, document.LeadDocumentId)).FirstOrDefault());
          
          // Если документы выгружаются - для печати выгрузить PublicBody.
          // Если документы выгружаются в электронном виде - выгрузить PublicBody, оригинал и файл квалифицированной подписи.
          // Если требуются оригиналы документов - в pdf не преобразовывать.
          // В остальных случаях - преобразовать.
          if (!document.NeedConvert)
            AddVersionBodyToZipModel(version, fileName, version.PublicBody.Size == 0 ? false : true, folder, doc.Id, zipModels);
          else
          {
            if (version.PublicBody.Size == 0)
              this.ConvertVersionBodyAndAddToZipModel(document, version, fileName, folder, zipModels, doc);
            else
              AddVersionBodyToZipModel(version, fileName, true, folder, doc.Id, zipModels);
            if (!document.IsPrint)
            {
              AddVersionBodyToZipModel(version, fileName, false, folder, doc.Id, zipModels);
              // Получить подписи сотрудника и директора по персоналу.
              if (this.HasEmployeeSignInDocKind(doc) || this.HasComplectSignInDocKind(doc))
              {
                var employeeSign = GetEmployeeSignature(doc, true);
                ExportSignature(version, fileName + "__", folder, employeeSign, zipModels);
              }
              if (this.HasManagerSignInDocKind(doc) || this.HasComplectSignInDocKind(doc))
              {
                var managerSign = GetHRManagerSignature(doc, true);
                ExportSignature(version, fileName + "_1_", folder, managerSign, zipModels);
              }
            }
          }

          Logger.DebugFormat("Document with id '{0}' has been processed for export documents. For print: '{1}', lead document Id: '{2}', is faulted: '{3}', error message: '{4}'",
                             document.Id, document.IsPrint, document.LeadDocumentId, document.IsFaulted, document.Error);
        }
        catch (Exception ex)
        {
          Logger.Debug(ex.ToString());
          document.Error = Resources.ExportDialog_Error_ClientFormat(ex.Message.TrimEnd('.'));
          document.IsFaulted = true;
        }
      }
      
      result.ExportedDocuments = expDocuments;
      result.ZipModels = zipModels;
      return result;
    }
    
    /// <summary>
    /// Добавить тело версии в модель выгрузки.
    /// </summary>
    /// <param name="version">Версия документа.</param>
    /// <param name="docName">Имя документа.</param>
    /// <param name="isPublicBody">Признак вызрузки Body или PublicBody. True - выгружать PublicBody.</param>
    /// <param name="folder">Папка.</param>
    /// <param name="id">ИД документа.</param>
    /// <param name="zipModels">Список моделей.</param>
    private static void AddVersionBodyToZipModel(Sungero.Content.IElectronicDocumentVersions version, string docName, bool isPublicBody,
                                                 Structures.Module.ExportedFolder folder, int id,
                                                 List<Structures.Module.ZipModel> zipModels)
    {
      
      var body = isPublicBody ? version.PublicBody : version.Body;
      var extension = isPublicBody ? version.AssociatedApplication.Extension : version.BodyAssociatedApplication.Extension;
      var fileName = docName + "." + extension;
      var zipModel = Structures.Module.ZipModel.Create();
      zipModel.DocumentId = id;
      zipModel.VersionId = version.Id;
      zipModel.IsPublicBody = isPublicBody;
      zipModel.FileName = fileName;
      zipModel.FolderRelativePath = GetFolderRelativePath(folder).ToList();
      zipModel.Size = body.Size;
      zipModels.Add(zipModel);
    }
    
    /// <summary>
    /// Преобразовать тело версии в Pdf и добавить в модель выгрузки.
    /// </summary>
    /// <param name="exportedDoc">Описание документа.</param>
    /// <param name="version">Версия.</param>
    /// <param name="docName">Имя документа.</param>
    /// <param name="folder">Папка.</param>
    /// <param name="zipModels">Список моделей.</param>
    /// <param name="document">Документ.</param>
    private void ConvertVersionBodyAndAddToZipModel(Structures.Module.ExportedDocument exportedDoc, Sungero.Content.IElectronicDocumentVersions version,
                                                    string docName, Structures.Module.ExportedFolder folder, List<Structures.Module.ZipModel> zipModels,
                                                    IOfficialDocument document)
    {
      var employeeAnchors = new List<string>() { HRManagement.Resources.EmployeeAnchor };
      var ourSignatorAnchors = new List<string>() { HRManagement.Resources.ManagerAnchor };
      var employees = new List<IEmployee>() { HRDocuments.Is(document) ? HRDocuments.As(document).Employee : HROrders.Is(document) ? HROrders.As(document).Employee : Employees.Null };
      var ourSignatories = new List<IEmployee>() { document.OurSignatory };
      var errorMessage = string.Empty;
      System.IO.Stream pdfDocumentStream = null;
      using (var inputStream = new System.IO.MemoryStream())
      {
        version.Body.Read().CopyTo(inputStream);
        try
        {
          pdfDocumentStream = this.GeneratePdfDocumentStream(inputStream, employees, employeeAnchors, ourSignatories, ourSignatorAnchors, version, document);
          var pdfDocStream = new System.IO.MemoryStream();
          pdfDocumentStream.CopyTo(pdfDocStream);
          
          var zipModel = Structures.Module.ZipModel.Create();
          zipModel.DocumentId = exportedDoc.Id;
          zipModel.VersionId = version.Id;
          zipModel.IsPublicBody = true;
          zipModel.FileName = docName + ".pdf";
          zipModel.FolderRelativePath = GetFolderRelativePath(folder).ToList();
          zipModel.Size = pdfDocStream.ToArray().Count();
          zipModel.Body = pdfDocStream.ToArray();
          zipModels.Add(zipModel);
          
          pdfDocumentStream.Close();
        }
        catch (Exception e)
        {
          if (e is Sungero.AsposeExtensions.PdfConvertException)
          {
            errorMessage = Sungero.Docflow.Resources.PdfConvertErrorFormat(exportedDoc.Id);
            Logger.Error(errorMessage);
          }
          else
          {
            errorMessage = string.Format("{0} {1}", Sungero.Docflow.Resources.PdfConvertErrorFormat(exportedDoc.Id), e.Message);
            Logger.Error(errorMessage);
          }
          exportedDoc.IsFaulted = true;
          exportedDoc.Error = Sungero.Docflow.Resources.PdfConvertErrorFormat(docName);
        }
      }
    }
    
    /// <summary>
    /// Добавить отметки подписей в поток данных версии документа.
    /// </summary>
    /// <param name="inputStream">Поток данных.</param>
    /// <param name="employees">Список сотрудников.</param>
    /// <param name="employeeAnchors">Список якорей.</param>
    /// <param name="ourSignatories">Список подписантов от организации.</param>
    /// <param name="ourSignatorAnchors">Список якорей для подписантов от организации.</param>
    /// <param name="version">Версия документа.</param>
    /// <param name="document">Документ.</param>
    /// <returns>Поток данных с отметками подписи.</returns>
    public System.IO.Stream GeneratePdfDocumentStream(System.IO.MemoryStream inputStream,
                                                      List<IEmployee> employees, List<string> employeeAnchors,
                                                      List<IEmployee> ourSignatories, List<string> ourSignatorAnchors,
                                                      Sungero.Content.IElectronicDocumentVersions version,
                                                      IOfficialDocument document)
    {
      var pdfConverter = new Sungero.AsposeExtensions.Converter();
      var extension = version.BodyAssociatedApplication.Extension;
      System.IO.Stream pdfDocumentStream = pdfConverter.GeneratePdf(inputStream, extension);
      
      // Если на виде документа предусмотрена подпись сотрудника, для каждого сотрудника найти подпись в соответствии с настройками и проставить в документ в соответствии с якорем.
      if (this.HasEmployeeSignInDocKind(document) || this.HasComplectSignInDocKind(document))
        pdfDocumentStream = this.AddSignaturesByAnchors(pdfDocumentStream, pdfConverter, extension, employees, employeeAnchors, document);
      
      // Если на виде документа предусмотрена подпись подписанта, получить их подписи.
      if (this.HasManagerSignInDocKind(document) || this.HasComplectSignInDocKind(document))
        pdfDocumentStream = this.AddSignaturesByAnchors(pdfDocumentStream, pdfConverter, extension, ourSignatories, ourSignatorAnchors, document, true);
      
      return pdfDocumentStream;
    }
    
    /// <summary>
    /// Найти подписи и проставить в документ в соответствии с якорями.
    /// </summary>
    /// <param name="pdfDocumentStream">Поток данных для отметки подписей.</param>
    /// <param name="pdfConverter">PDF конвертер.</param>
    /// <param name="extension">Расширение файла.</param>
    /// <param name="employees">Список сотрудников.</param>
    /// <param name="employeeAnchors">Список якорей.</param>
    /// <param name="document">Документ.</param>
    /// <param name="isOurSignator">True, если сотрудники являются подписантами от организации.</param>
    /// <returns>Поток данных с отметками подписи.</returns>
    public System.IO.Stream AddSignaturesByAnchors(System.IO.Stream pdfDocumentStream,
                                                   Sungero.AsposeExtensions.Converter pdfConverter,
                                                   string extension,
                                                   List<IEmployee> employees,
                                                   List<string> employeeAnchors,
                                                   IOfficialDocument document,
                                                   bool isOurSignator = false)
    {
      for (var i = 0; i < employees.Count(); i++)
      {
        var needAdvancedSign = isOurSignator ? Functions.Module.NeedStaffChiefAdvancedSign(employees[i]) : Functions.Module.IsQualifiedSignature(employees[i]);
        var employeeSignature = GetEmployeesSignaturesForMarksAsHtml(employees[i], document, needAdvancedSign);
        if (employeeSignature != string.Empty)
          pdfDocumentStream = pdfConverter.AddSignatureMark(pdfDocumentStream, extension, employeeSignature, employeeAnchors[i], Constants.Module.SearchablePagesLimit);
      }
      return pdfDocumentStream;
    }
    
    /// <summary>
    /// Добавить квалифицированные подписи в модель выгрузки.
    /// </summary>
    /// <param name="version">Версия документа.</param>
    /// <param name="docName">Имя документа.</param>
    /// <param name="folder">Папка.</param>
    /// <param name="signature">Подпись.</param>
    /// <param name="zipModels">Список моделей.</param>
    private static void ExportSignature(Sungero.Content.IElectronicDocumentVersions version, string docName, Structures.Module.ExportedFolder folder,
                                        Sungero.Domain.Shared.ISignature signature, List<Structures.Module.ZipModel> zipModels)
    {
      if (signature != null && signature.SignCertificate != null)
      {
        var signData = signature.GetDataSignature();
        var signFullFileName = docName + "SGN" + ".sgn";
        
        if (zipModels != null)
        {
          var zipModel = Structures.Module.ZipModel.Create();
          zipModel.DocumentId = version.ElectronicDocument.Id;
          zipModel.VersionId = version.Id;
          zipModel.FileName = signFullFileName;
          zipModel.FolderRelativePath = GetFolderRelativePath(folder).ToList();
          zipModel.SignatureId = signature.Id;
          zipModel.Size = signData.LongLength;
          zipModels.Add(zipModel);
          signData = null;
        }
      }
    }
    
    /// <summary>
    /// Получить относительный путь.
    /// </summary>
    /// <param name="folder">Папка.</param>
    /// <returns>Путь до файла.</returns>
    private static string[] GetFolderRelativePath(Structures.Module.ExportedFolder folder)
    {
      if (string.IsNullOrWhiteSpace(folder.FolderName))
        return new string[0];
      
      if (string.IsNullOrWhiteSpace(folder.ParentRelativePath))
        return new string[] { folder.FolderName };
      
      return new string[] { folder.ParentRelativePath, folder.FolderName };
    }
    
    /// <summary>
    /// Создать ZIP архив со всеми выгружаемыми документами.
    /// </summary>
    /// <param name="zipModels">Список моделей.</param>
    /// <param name="documents">Выгружаемые документы.</param>
    /// <returns>ZIP архив.</returns>
    [Remote]
    public static IZip CreateZipFromZipModel(List<Structures.Module.ZipModel> zipModels, List<Structures.Module.ExportedDocument> documents)
    {
      var zip = Zip.Create();
      foreach (var zipModel in zipModels)
      {
        if (zipModel.Body == null)
        {
          var document = Sungero.Docflow.OfficialDocuments.Get(zipModel.DocumentId);
          var version = document.Versions.Where(x => x.Id == zipModel.VersionId).FirstOrDefault();
          if (zipModel.SignatureId != null)
          {
            var signature = Signatures.Get(version).Where(x => x.Id == zipModel.SignatureId).SingleOrDefault();
            zip.Add(signature, zipModel.FileName, zipModel.FolderRelativePath.ToArray());
            continue;
          }
          var body = zipModel.IsPublicBody ? version.PublicBody : version.Body;
          zip.Add(body, zipModel.FileName, zipModel.FolderRelativePath.ToArray());
        }
        else
        {
          // HACK Преобразовать тип к IInternalZip, чтобы можно было добавить массив байт в zip. Убрать после доработки платформы
          var internalZip = (Sungero.Domain.IInternalZip)zip;
          internalZip.Add(zipModel.Body, zipModel.FileName, "pdf", zipModel.FolderRelativePath.ToArray());
        }
        Logger.DebugFormat("Document with Id '{0}', version id '{1}', is PublicBody: '{2}' has been added to zip model",
                           zipModel.DocumentId, zipModel.VersionId, zipModel.IsPublicBody);
      }
      
      // Создать лог-файл и добавить его к архиву.
      if (documents.Any(d => d.IsFaulted))
      {
        var errorText = new List<string>();
        foreach (var errorDoc in documents.Where(d => d.IsFaulted))
        {
          errorText.Add(Resources.ExportLogTemplateFormat(errorDoc.Id, errorDoc.Name, errorDoc.Error));
        }
        var errorAllText = string.Join(Environment.NewLine, errorText.ToArray());
        using (var logFileStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(errorAllText)))
        {
          // HACK Преобразовать тип к IInternalZip, чтобы можно было добавить массив байт в zip. Убрать после доработки платформы
          var internalZip = (Sungero.Domain.IInternalZip)zip;
          internalZip.Add(logFileStream.ToArray(), "Log", "log", (new List<string>()).ToArray());
        }
      }
      
      var now = Calendar.UserNow;
      var tempFolderName = Resources.ExportDocumentFolderNameFormat(now.ToShortDateString() + " " + now.ToLongTimeString()).ToString();
      tempFolderName = CommonLibrary.FileUtils.NormalizeFileName(tempFolderName);
      zip.Save(tempFolderName);
      return zip;
    }
    
    /// <summary>
    /// Получить подпись Директора по персоналу, или сотрудника, который ставит подпись от его лица.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="signLastVersion">True, если подпись должна стоять на последней версии документа, иначе на одной из версий.</param>
    /// <returns>Подпись.</returns>
    public static Sungero.Domain.Shared.ISignature GetHRManagerSignature(IOfficialDocument document, bool signLastVersion)
    {
      var ourSignatory = document.OurSignatory;
      if (ourSignatory == null)
        return null;
      
      if (signLastVersion)
        return GetVersionSignature(ourSignatory, document.LastVersion, Functions.Module.NeedStaffChiefAdvancedSign(ourSignatory));
      else
      {
        foreach (var version in document.Versions)
        {
          var signature = GetVersionSignature(ourSignatory, version, Functions.Module.NeedStaffChiefAdvancedSign(ourSignatory));
          if (signature != null)
            return signature;
        }
        return null;
      }
    }

    /// <summary>
    /// Получить подпись сотрудника.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="signLastVersion">True, если подпись должна стоять на последней версии документа, иначе на одной из версий.</param>
    /// <returns>Подпись.</returns>
    public static Sungero.Domain.Shared.ISignature GetEmployeeSignature(IOfficialDocument document, bool signLastVersion)
    {
      var employee = HRDocuments.Is(document) ? HRDocuments.As(document).Employee : HROrders.Is(document) ? HROrders.As(document).Employee : Employees.Null;
      if (employee != null)
      {
        if (signLastVersion)
          return GetVersionSignature(employee, document.LastVersion, Functions.Module.IsQualifiedSignature(employee));
        else
        {
          foreach (var version in document.Versions)
          {
            var signature = GetVersionSignature(employee, version, Functions.Module.IsQualifiedSignature(employee));
            if (signature != null)
              return signature;
          }
        }
      }
      return null;
    }
    
    /// <summary>
    /// Проверить наличие полного комплекта подписей на документе. Для приказов, трудового договора и доп. соглашения должны стоять обе подписи для полного комплекта.
    /// Для уведомления об отпуске - подпись сотрудника.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="signLastVersion">True, если подпись должна стоять на последней версии документа, иначе на одной из версий.</param>
    /// <returns>True, если на документе полный комплект подписей.</returns>
    public virtual bool IsFullComplectInDocument(IOfficialDocument document, bool signLastVersion)
    {
      var managerSign = GetHRManagerSignature(document, signLastVersion);
      var employeeSign = GetEmployeeSignature(document, signLastVersion);
      var isFullComplect = true;
      if (this.HasComplectSignInDocKind(document) && (managerSign != null && employeeSign == null) || (document.DocumentKind == DocKindFunctions.GetNativeDocumentKind(DocKind.ScheduledVacationNoticeKind) && employeeSign == null))
        isFullComplect = false;
      return isFullComplect;
    }
    
    /// <summary>
    /// Проверить, полный ли комплект подписей должен быть на виде документа. Полный комплект - подпись и сотрудника и подписанта.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>True, если вид документа удовлетворяет проверке.</returns>
    public virtual bool HasComplectSignInDocKind(IOfficialDocument document)
    {
      var docKindGuid = GetDocumentKindGuid(document.DocumentKind);
      return docKindGuid == DocKind.EmploymentContractKind || docKindGuid == DocKind.TransferAdditionalAgreementKind ||
        docKindGuid == DocKind.ChangeWorkConditionsAdditionalAgreementKind || docKindGuid == DocKind.HiringOrderKind ||
        docKindGuid == DocKind.TransferOrderKind || docKindGuid == DocKind.ChangeWorkConditionsOrderKind ||
        docKindGuid == DocKind.DismissalOrderKind || docKindGuid == DocKind.VacationOrderKind ||
        docKindGuid == DocKind.VacationShiftOrderKind || docKindGuid == DocKind.VacationRecallOrderKind;
    }
    
    /// <summary>
    /// Проверить, что для данного вида документа должна быть только подпись сотрудника.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>True, если вид документа удовлетворяет проверке.</returns>
    public virtual bool HasEmployeeSignInDocKind(IOfficialDocument document)
    {
      var docKindGuid = GetDocumentKindGuid(document.DocumentKind);
      return docKindGuid == DocKind.AcquaintanceListKind || docKindGuid == DocKind.TransferStatementKind ||
        docKindGuid == DocKind.DismissalStatementKind || docKindGuid == DocKind.ScheduledVacationNoticeKind ||
        docKindGuid == DocKind.VacationStatementKind || docKindGuid == DocKind.VacationShiftStatementKind ||
        docKindGuid == DocKind.VacationRecallStatementKind;
    }
    
    /// <summary>
    /// Проверить, что для данного вида документа должна быть только подпись подписанта.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>True, если вид документа удовлетворяет проверке.</returns>
    public virtual bool HasManagerSignInDocKind(IOfficialDocument document)
    {
      var docKindGuid = GetDocumentKindGuid(document.DocumentKind);
      return docKindGuid == DocKind.VacationScheduleKind;
    }
    
    /// <summary>
    /// Проверить, нужно ли конвертировать документа в PDF. Если требуются оригиналы бумажных документов - выгружать без преобразования.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>True, если нужно преобразовывать в pdf.</returns>
    public virtual bool NeedConvertToPdf(IOfficialDocument document)
    {
      var docKind = document.DocumentKind;
      var busUnit = document.BusinessUnit;
      // Документ относится к движению персонала.
      // Иначе - документ относится к отпускам.
      if (GetHRDocumentKinds(false).Contains(docKind))
      {
        if (this.IsAllPaperStaffDoc(busUnit))
          return false;
        else if (this.IsOnlyBilStaffDocPaper(busUnit))
          return docKind != DocKindFunctions.GetNativeDocumentKind(DocKind.EmploymentContractKind) &&
            docKind != DocKindFunctions.GetNativeDocumentKind(DocKind.TransferAdditionalAgreementKind) &&
            docKind != DocKindFunctions.GetNativeDocumentKind(DocKind.ChangeWorkConditionsAdditionalAgreementKind);
        else
          return true;
      }
      else
      {
        if (this.IsAllPaperVacationDoc(busUnit))
          return false;
        else return true;
      }
    }
    #endregion
    
    #region Работа с ЭП.
    
    /// <summary>
    /// Получить все данные для валидации подписания одним запросом.
    /// </summary>
    /// <param name="documents">Список документов.</param>
    /// <returns>Список структур с данными валидации.</returns>
    [Remote(IsPure = true)]
    public List<Structures.Module.BeforeSign> ValidateBeforeSign(List<IOfficialDocument> documents)
    {
      var beforeSign = new List<Structures.Module.BeforeSign>();
      var currentEmployee = Sungero.Company.Employees.Current;
      foreach (var document in documents)
      {
        var signatories = new List<Sungero.Docflow.Structures.SignatureSetting.Signatory>();
        var errors = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetApprovalValidationErrors(document, false);
        if (HROrders.Is(document))
          signatories = Functions.HROrder.GetSignatories(HROrders.As(document));
        else
          signatories = Functions.HRDocumentBase.GetSignatories(HRDocumentBases.As(document));
        var canApprove = document.AccessRights.CanApprove() && signatories.Any(s => Equals(s.EmployeeId, currentEmployee.Id));
        var bodyChanged = DocumentHasBodyUpdateAfterLastView(document);
        beforeSign.Add(Structures.Module.BeforeSign.Create(errors, canApprove, bodyChanged));
      }
      return beforeSign;
    }
    
    /// <summary>
    /// Проверить есть ли действительные сертификаты для подписания документов у сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>True - если есть действительные сертификаты.</returns>
    [Remote]
    public bool HasValidCertificate(IEmployee employee)
    {
      return Sungero.Company.PublicFunctions.Module.Remote.GetCertificatesOfEmployee(employee).Where(d => d.Enabled == true).Any(d => !d.NotAfter.HasValue || d.NotAfter.Value > Calendar.Now);
    }
    
    /// <summary>
    /// Проверить есть ли сертификаты для подписания документов.
    /// </summary>
    /// <param name="documents">Список документов.</param>
    /// <returns>True - если есть сертификаты для каждого документа, false - если для какого-то нет.</returns>
    [Remote]
    public bool HasCertificates(List<IOfficialDocument> documents)
    {
      foreach (var document in documents)
        if (!Sungero.Docflow.PublicFunctions.Module.Remote.GetCertificates(document).Any())
          return false;
      return true;
    }

    /// <summary>
    /// Получить утверждающую подпись сотрудника в формате Html для простановки в тело документа.
    /// Сначала ищется подпись сотрудника, затем по замещению. Квалифицированная подпись, если ее нет - простая. Если подписей несколько - последняя по времени.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="document">Документ.</param>
    /// <param name="useAdvanceSign">Признак использования усиленной подписи.</param>
    /// <returns>Электронная подпись сотрудника в формате Html. Если подписи сотрудника на документе нет, то пустая строка.</returns>
    /// <remarks>Поиск реализован по аналогии с GetSignatureForMark из Sungero.Docflow.Server\OfficialDocument\OfficialDocumentServerFunctions.</remarks>
    public static string GetEmployeesSignaturesForMarksAsHtml(IEmployee employee, IOfficialDocument document, bool useAdvanceSign)
    {
      var version = document.LastVersion;
      
      if (employee != null)
      {
        var signature = GetVersionSignature(employee, document.LastVersion, useAdvanceSign);
        Logger.DebugFormat("==>> GetEmployeesSignaturesForMarksAsHtml employee {0},  signature {1},  signature != null {2}", employee, signature, signature != null);
        
        if (signature != null)
        {
          // В случае квалифицированной ЭП информацию для отметки брать из атрибутов субъекта сертификата.
          if (signature.SignCertificate != null)
            return GetSignatureMarkForCertificateAsHtml(signature);
          // В случае простой ЭП информацию для отметки брать из атрибутов подписи.
          else
            return GetSignatureMarkForSimpleSignatureAsHtml(signature);
        }
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Получить утверждающую подпись сотрудника.
    /// Сначала ищется подпись сотрудника, затем по замещению. Квалифицированная подпись, если ее нет - простая. Если подписей несколько - последняя по времени.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="version">Версия документа.</param>
    /// <param name="useAdvanceSign">Признак использования усиленной подписи.</param>
    /// <returns>Подпись сотрудника.</returns>
    public static Sungero.Domain.Shared.ISignature GetVersionSignature(IEmployee employee, IElectronicDocumentVersions version, bool useAdvanceSign)
    {
      var versionSignatures = Signatures.Get(version).Where(s => s.SignatureType == SignatureType.Approval
                                                            && (Equals(employee, s.Signatory) || (s.Signatory == null && s.SignatoryFullName == employee.Name) || Equals(employee, s.SubstitutedUser))).ToList();
      if (versionSignatures.Any())
      {
        if (useAdvanceSign)
          // Своя подпись важнее замещения, квалифицированная приоритетнее простой, взять последнюю по времени.
          return versionSignatures.OrderBy(s => Equals(employee, s.Signatory) || (s.Signatory == null && s.SignatoryFullName == employee.Name))
            .ThenBy(s => s.SignCertificate == null).ThenByDescending(s => s.SigningDate).FirstOrDefault();
        else
          // Своя подпись важнее замещения, простая приоритетнее квалифицированной, взять последнюю по времени.
          return versionSignatures.OrderBy(s => Equals(employee, s.Signatory) || (s.Signatory == null && s.SignatoryFullName == employee.Name))
            .ThenBy(s => s.SignCertificate != null).ThenByDescending(s => s.SigningDate).FirstOrDefault();
      }
      else
        return null;
    }
    
    /// <summary>
    /// Получить отметку об ЭП для сертификата из подписи.
    /// </summary>
    /// <param name="signature">Подпись.</param>
    /// <returns>Изображение отметки об ЭП для сертификата в виде html.</returns>
    /// <remarks>Скопирован метод Docflow получения отметки об ЭП для сертификата GetSignatureMarkForCertificateAsHtml. Добавлена дата подписания.</remarks>
    public static string GetSignatureMarkForCertificateAsHtml(Sungero.Domain.Shared.ISignature signature)
    {
      if (signature == null)
        return string.Empty;
      
      var certificate = signature.SignCertificate;
      if (certificate == null)
        return string.Empty;
      
      var certificateSubject = Sungero.Docflow.PublicFunctions.Module.GetCertificateSubject(signature);
      
      var signatoryFullName = string.Format("{0} {1}", certificateSubject.Surname, certificateSubject.GivenName).Trim();
      if (string.IsNullOrEmpty(signatoryFullName))
        signatoryFullName = certificateSubject.CounterpartyName;
      
      string html = Resources.HtmlStampTemplateForCertificate;
      html = html.Replace("{SignatoryFullName}", signatoryFullName);
      html = html.Replace("{Thumbprint}", certificate.Thumbprint.ToLower());
      var validity = string.Format("{0} {1} {2} {3}",
                                   Sungero.Company.Resources.From,
                                   certificate.NotBefore.Value.ToShortDateString(),
                                   Sungero.Company.Resources.To,
                                   certificate.NotAfter.Value.ToShortDateString());
      html = html.Replace("{Validity}", validity);
      html = html.Replace("{SigningDate}", signature.SigningDate.ToShortDateString());
      return html;
    }
    
    /// <summary>
    /// Получить отметку об ЭП для подписи.
    /// </summary>
    /// <param name="signature">Подпись.</param>
    /// <returns>Изображение отметки об ЭП для подписи в виде html.</returns>
    /// <remarks>Скопирован метод Docflow получения отметки об ЭП для подписи GetSignatureMarkForSimpleSignatureAsHtml. Добавлена дата подписания.</remarks>
    public static string GetSignatureMarkForSimpleSignatureAsHtml(Sungero.Domain.Shared.ISignature signature)
    {
      if (signature == null)
        return string.Empty;
      
      var signatoryFullName = signature.SignatoryFullName;
      var signatoryId = signature.Signatory.Id;
      // Подменить пользователя "Система" на пользователя из задачи.
      if (Functions.Module.IsServiceUser(signature.Signatory))
      {
        signatoryFullName = HRManagement.Resources.SignedInPAFormat(signature.SubstitutedUserFullName);
        signatoryId = signature.SubstitutedUser.Id;
      }
      
      string html = Resources.HtmlStampTemplateForSignature;
      html = html.Replace("{SignatoryFullName}", signatoryFullName);
      html = html.Replace("{SignatoryId}", signatoryId.ToString());
      html = html.Replace("{SigningDate}", signature.SigningDate.ToShortDateString());
      return html;
    }
    
    /// <summary>
    /// Преобразовать документ в PDF и поставить отметки об ЭП.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="employees">Список сотрудников.</param>
    /// <param name="employeeAnchors">Список якорей.</param>
    /// <returns>Текст ошибки или пустая строка, если функция отработала без ошибок.</returns>
    /// <remarks>Скопирована серверная функция Docflow генерации PublicBody для версии документа GeneratePublicBodyWithSignatureMark.
    /// Добавлена возможность ставить несколько штампов, изменен тип возвращаемого занчения на строку.</remarks>
    [Public]
    public virtual Structures.Module.IСonversionToPdfResult ConvertToPdfWithSignatureMarks(IOfficialDocument document, List<IEmployee> employees, List<string> employeeAnchors)
    {
      var ourSignatories = new List<IEmployee>() { };
      var ourSignatoryAnchors = new List<string>() { };
      return this.ConvertToPdfWithSignatureMarks(document, employees, employeeAnchors, ourSignatories, ourSignatoryAnchors);
    }

    /// <summary>
    /// Преобразовать документ в PDF и поставить отметки об ЭП.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="employees">Список сотрудников.</param>
    /// <param name="employeeAnchors">Список якорей.</param>
    /// <param name="ourSignatories">Список подписантов от организации.</param>
    /// <param name="ourSignatorAnchors">Список якорей для подписантов от организации.</param>
    /// <returns>Результат преобразования.</returns>
    /// <remarks>Скопирована серверная функция Docflow генерации PublicBody для версии документа GeneratePublicBodyWithSignatureMark.
    /// Добавлена возможность ставить несколько штампов, изменен тип возвращаемого занчения на строку.</remarks>
    [Public]
    public virtual Structures.Module.IСonversionToPdfResult ConvertToPdfWithSignatureMarks(IOfficialDocument document, List<IEmployee> employees, List<string> employeeAnchors, List<IEmployee> ourSignatories, List<string> ourSignatorAnchors)
    {
      var info = Structures.Module.СonversionToPdfResult.Create();
      info.HasErrors = true;
      var versionId = document.LastVersion.Id;
      var version = document.Versions.SingleOrDefault(v => v.Id == versionId);
      if (version == null)
      {
        info.HasConvertionError = true;
        info.ErrorMessage = OfficialDocuments.Resources.NoVersionWithNumberErrorFormat(versionId);
        return info;
      }
      
      var errorMessage = string.Empty;
      System.IO.Stream pdfDocumentStream = null;
      using (var inputStream = new System.IO.MemoryStream())
      {
        version.Body.Read().CopyTo(inputStream);
        try
        {
          pdfDocumentStream = this.GeneratePdfDocumentStream(inputStream, employees, employeeAnchors, ourSignatories, ourSignatorAnchors, version, document);
        }
        catch (Exception e)
        {
          if (e is Sungero.AsposeExtensions.PdfConvertException)
          {
            errorMessage = Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id);
            Logger.Error(errorMessage);
          }
          else
          {
            errorMessage = string.Format("{0} {1}", Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), e.Message);
            Logger.Error(errorMessage);
          }
          
          info.HasConvertionError = true;
          info.HasLockError = false;
          info.ErrorMessage = errorMessage;
        }
      }
      
      if (!string.IsNullOrWhiteSpace(info.ErrorMessage))
        return info;
      
      version.PublicBody.Write(pdfDocumentStream);
      version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
      pdfDocumentStream.Close();
      
      try
      {
        document.Save();
        info.HasErrors = false;
      }
      catch (Sungero.Domain.Shared.Exceptions.RepeatedLockException e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = false;
        info.HasLockError = true;
        info.ErrorMessage = e.Message;
      }
      catch (Exception e)
      {
        Logger.Error(e.Message);
        info.HasConvertionError = true;
        info.HasLockError = false;
        info.ErrorMessage = e.Message;
      }
      
      return info;
    }
    #endregion
    
    #region Проверка принадлежности пользователя к роли.
    
    /// <summary>
    /// Проверить, что пользователь включен в роль.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <param name="roleGuid">Guid роли.</param>
    /// <returns>True, если один из пользователей входит в роль.</returns>
    public bool IsIncludedInRole(List<IUser> users, Guid roleGuid)
    {
      var role = Functions.Module.GetRole(roleGuid);
      
      if (role == null)
        return false;
      return users.Any(v => v.IncludedIn(role));
    }
    
    /// <summary>
    /// Проверить, является ли пользователь служебным.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <returns>True, если пользователь служебный.</returns>
    [Public, Remote]
    public bool IsServiceUser(IUser user)
    {
      return this.IsServiceUser(new List<IUser> { user });
    }
    
    /// <summary>
    /// Проверить, является ли пользователь служебным.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>True, если один из пользователей служебный.</returns>
    [Public, Remote]
    public bool IsServiceUser(List<IUser> users)
    {
      return this.IsIncludedInRole(users, Sungero.Domain.Shared.SystemRoleSid.ServiceUsers);
    }
    
    /// <summary>
    /// Проверить, что пользователь включен в роль Администратор HR-процессов.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>True, если один из пользователей входит в роль Администратор HR-процессов.</returns>
    [Remote(IsPure = true), Public]
    public bool IsHRAdministrator(List<IUser> users)
    {
      return this.IsIncludedInRole(users, HRRoles.HRAdministrator);
    }
    
    /// <summary>
    /// Проверить, что пользователь включен в роль Директор по персоналу.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>True, если один из пользователей входит в роль Директор по персоналу.</returns>
    [Remote(IsPure = true), Public]
    public bool IsStaffChief(List<IUser> users)
    {
      return this.IsIncludedInRole(users, HRRoles.StaffChief);
    }
    
    /// <summary>
    /// Проверить, что пользователь включен в роль Специалист по работе с персоналом.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>True, если один из пользователей входит в роль Специалист по работе с персоналом.</returns>
    [Remote(IsPure = true), Public]
    public bool IsHRResponsible(List<IUser> users)
    {
      return this.IsIncludedInRole(users, DirRX.HRLite.PublicConstants.Module.Roles.HRSpecialist);
    }
    
    /// <summary>
    /// Проверить, что пользователь включен в роль Отвественный за выгрузку кадровых документов.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>True, если один из пользователей входит в роль Отвественный за выгрузку кадровых документов.</returns>
    [Remote(IsPure = true), Public]
    public bool IsHRDocExportManager(List<IUser> users)
    {
      return this.IsIncludedInRole(users, HRRoles.HRDocExportManager);
    }
    
    /// <summary>
    /// Проверить, что пользователь включен в роль Отвественный за график отпусков по организации.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>True, если один из пользователей входит в роль Отвественный за график отпусков по организации.</returns>
    [Remote(IsPure = true), Public]
    public bool IsVacationResponsible(List<IUser> users)
    {
      return this.IsIncludedInRole(users, HRRoles.VacationResponsible);
    }
    
    /// <summary>
    /// Проверить, что пользователь включен в роль Отвественный за график отпусков в подразделении.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>True, если один из пользователей входит в роль Отвественный за график отпусков в подразделении.</returns>
    [Remote(IsPure = true), Public]
    public bool IsVacationManager(List<IUser> users)
    {
      return this.IsIncludedInRole(users, HRRoles.VacationManagers);
    }
    
    #endregion
    
    #region Функции для работы с ролями.
    
    /// <summary>
    /// Получить список всех ролей модуля.
    /// </summary>
    /// <returns>Список ролей.</returns>
    [Remote, Public]
    public IQueryable<IRole> GetHRRolesList()
    {
      var roleGuids = new List<Guid>();
      roleGuids.Add(HRRoles.Accounter);
      roleGuids.Add(DirRX.HRLite.PublicConstants.Module.Roles.HRSpecialist);
      roleGuids.Add(HRRoles.ITResponsible);
      roleGuids.Add(HRRoles.SecurityResponsible);
      roleGuids.Add(HRRoles.StaffChief);
      roleGuids.Add(HRRoles.SupplyResponsible);
      roleGuids.Add(HRRoles.VacationManagers);
      roleGuids.Add(HRRoles.VacationResponsible);
      roleGuids.Add(HRRoles.HRSettingsManager);
      roleGuids.Add(HRRoles.HRAdministrator);
      roleGuids.Add(HRRoles.HRDocExportManager);
      return Roles.GetAll(l => roleGuids.Contains(l.Sid.Value));
    }
    
    /// <summary>
    /// Назначить исполнителей роли.
    /// </summary>
    /// <param name="role">Роль.</param>
    /// <param name="newMembers">Исполнители роли.</param>
    [Public, Remote]
    public void SetRecpientsRole(IRole role, List<IRecipient> newMembers)
    {
      role.RecipientLinks.Clear();
      foreach (var member in newMembers)
      {
        var row = role.RecipientLinks.AddNew();
        row.Member = member;
      }
      role.Save();
    }
    
    /// <summary>
    /// Получить исполнителей роли.
    /// </summary>
    /// <param name="roleGuid">Guid роли.</param>
    /// <param name="processedRoles">Список guid'ов обработанных ролей.</param>
    /// <returns>Список исполнителей.</returns>
    [Public]
    public virtual List<IRecipient> GetRoleRecipients(Guid roleGuid, List<Guid> processedRoles)
    {
      var recipientList = new List<IRecipient>();
      if (!processedRoles.Contains(roleGuid))
      {
        processedRoles.Add(roleGuid);
        var role = Functions.Module.GetRole(roleGuid);
        if (role != null)
        {
          foreach (var recipient in role.RecipientLinks)
          {
            var member = recipient.Member;
            if (Users.Is(member))
            {
              // Добавить в список пользователей.
              if (!this.IsServiceUser(Users.As(member)))
                recipientList.Add(member);
            }
            else if (Roles.Is(member))
              // Развернуть роль до конечных пользователей и добавить их в список.
              recipientList.AddRange(this.GetRoleRecipients(Roles.As(member).Sid.Value, processedRoles));
            else if (Departments.Is(member))
              // Добавить в список подразделения.
              recipientList.AddRange(this.GetDepartmentRecipients(Departments.As(member)));
          }
        }
        return recipientList.Where(l => l.Status == Sungero.CoreEntities.Recipient.Status.Active).Distinct().ToList();
      }
      else
        return recipientList;
    }
    
    /// <summary>
    /// Получить исполнителей роли.
    /// </summary>
    /// <param name="roleGuid">Guid роли.</param>
    /// <returns>Список исполнителей.</returns>
    [Public]
    public virtual List<IRecipient> GetRoleRecipients(Guid roleGuid)
    {
      return this.GetRoleRecipients(roleGuid, new List<Guid>());
    }
    
    /// <summary>
    /// Получить роль.
    /// </summary>
    /// <param name="roleGuid">Guid роли.</param>
    /// <returns>Роль.</returns>
    [Remote]
    public static IRole GetRole(Guid roleGuid)
    {
      return Roles.GetAll(r => r.Sid == roleGuid).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить исполнителя роли в нашей организации.
    /// </summary>
    /// <param name="recipients">Список исполнителей.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Исполнитель роли в нашей организации.</returns>
    public virtual IRecipient GetRecipientForBusinessUnit(List<IRecipient> recipients, IBusinessUnit businessUnit)
    {
      if (recipients.Any())
        return recipients.Where(l => Equals(Employees.As(l).Department.BusinessUnit, businessUnit)).FirstOrDefault();
      else
        return Recipients.Null;
    }
    
    /// <summary>
    /// Проверить, является ли реципиент вычисляемой по НОР ролью.
    /// </summary>
    /// <param name="recipient">Реципиент.</param>
    /// <returns>
    /// True, если реципиент является вычисляемой по НОР ролью.
    /// </returns>
    /// <description>Вычисляемая по НОР роль - роль для которой отвественный опредляется динамически по НОР: СОК, Директор по персоналу и т.д.</description>
    public virtual bool IsCalculableRole(IRecipient recipient)
    {
      var role = Roles.As(recipient);
      if (role == null)
        return false;
      
      List<Guid> responsibleRoles = new List<Guid> { HRRoles.Accounter, HRRoles.VacationResponsible, HRRoles.StaffChief, DirRX.HRLite.PublicConstants.Module.Roles.HRSpecialist, HRRoles.ITResponsible,
        HRRoles.SupplyResponsible, HRRoles.SecurityResponsible };

      return responsibleRoles.Contains(role.Sid.Value);
    }
    
    /// <summary>
    /// Получить отвественного по НОР у роли.
    /// </summary>
    /// <param name="roleGuid">Guid роли.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Ответственный роли.</returns>
    public virtual IRecipient GetRoleRecipientForBusinessUnit(Guid roleGuid, IBusinessUnit businessUnit)
    {
      var recipients = this.GetRoleRecipients(roleGuid);
      return this.GetRecipientForBusinessUnit(recipients, businessUnit);
    }
    
    /// <summary>
    /// Получить ответственного за работу с графиком отпусков.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Исполнитель роли в выбранной нашей организации.</returns>
    [Public, Remote]
    public virtual IRecipient GetVacationResponsible(IBusinessUnit businessUnit)
    {
      return this.GetRoleRecipientForBusinessUnit(HRRoles.VacationResponsible, businessUnit);
    }
    
    /// <summary>
    /// Получить специалиста бухгалтерии.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Исполнитель роли в выбранной нашей организации.</returns>
    [Public]
    public virtual IRecipient GetAccounter(IBusinessUnit businessUnit)
    {
      return this.GetRoleRecipientForBusinessUnit(HRRoles.Accounter, businessUnit);
    }
    
    /// <summary>
    /// Получить директора по персоналу.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Исполнитель роли в выбранной нашей организации.</returns>
    [Remote, Public]
    public virtual IRecipient GetStaffChief(IBusinessUnit businessUnit)
    {
      return this.GetRoleRecipientForBusinessUnit(HRRoles.StaffChief, businessUnit);
    }

    /// <summary>
    /// Получить специалиста по работе с персоналом.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Исполнитель роли в выбранной нашей организации.</returns>
    [Remote, Public]
    public virtual IRecipient GetHRResponsible(IBusinessUnit businessUnit)
    {
      return this.GetRoleRecipientForBusinessUnit(DirRX.HRLite.PublicConstants.Module.Roles.HRSpecialist, businessUnit);
    }
    
    /// <summary>
    /// Получить специалиста административно-хозяйственной службы.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Исполнитель роли в выбранной нашей организации.</returns>
    [Public]
    public virtual IRecipient GetSupplyResponsible(IBusinessUnit businessUnit)
    {
      return this.GetRoleRecipientForBusinessUnit(HRRoles.SupplyResponsible, businessUnit);
    }

    /// <summary>
    /// Получить специалиста службы безопасности.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Исполнитель роли в выбранной нашей организации.</returns>
    [Public]
    public virtual IRecipient GetSecurityResponsible(IBusinessUnit businessUnit)
    {
      return this.GetRoleRecipientForBusinessUnit(HRRoles.SecurityResponsible, businessUnit);
    }

    /// <summary>
    /// Получить специалиста ИТ-подразделения.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Исполнитель роли в выбранной нашей организации.</returns>
    [Public]
    public virtual IRecipient GetITResponsible(IBusinessUnit businessUnit)
    {
      return this.GetRoleRecipientForBusinessUnit(HRRoles.ITResponsible, businessUnit);
    }
    
    /// <summary>
    /// Определить руководителя сотрудника с учетом головных подразделений.
    /// По иерархии подразделений найти руководителя для текущего сотрудника.
    /// Если у подразделения указан руководитель отличный от переданного сотрудника, то вернуть его,
    ///   иначе проверять руководителей головных подразделений.
    /// Если среди головных подразделений не найден руководитель отличный от переданного сотрудника,
    ///   то проверить руководителя нашей организации.
    /// Если у нашей организации указан руководитель отличный от переданного сотрудника, то вернуть его,
    ///   иначе вернуть null.
    /// </summary>
    /// <param name="department">Подразделение.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Руководитель.</returns>
    public virtual IEmployee GetManager(IDepartment department, IEmployee employee)
    {
      var manager = department.Manager;
      var headOffice = department.HeadOffice;
      if (manager == null || Equals(employee, manager))
        if (headOffice != null)
          manager = this.GetManager(headOffice, employee);
      else {
        var businessUnit = department.BusinessUnit;
        if (businessUnit != null)
          manager = businessUnit.CEO;
        else
          manager = null;
      }
      return manager;
    }
    
    /// <summary>
    /// Получить руководителя сотрудника.
    /// Если в подразделении сотрудника руководитель не указан, то ищется ближайший руководитель по цепочке головных подразделений.
    /// Если не будет найден ни один руководитель в цепочке головных подразделений - веруть руководителя организации.
    /// Если руководитель не будет найден - вернуть null.
    /// Если переданный сотрудник является руководителем организации - вернуть его.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Руководитель.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IEmployee GetManager(IEmployee employee)
    {
      var department = employee.Department;
      return this.GetManager(department, employee);
    }
    
    /// <summary>
    /// Получить руководителя подразделения.
    /// Если в указанном подразделении руководитель не указан, то ищется ближайший руководитель по цепочке головных подразделений.
    /// Если не будет найден ни один руководитель в цепочке головных подразделений - вернуть руководителя организации.
    /// Веруть null, если руководитель не найден.
    /// </summary>
    /// <param name="department">Подразделение.</param>
    /// <returns>Руководитель.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IEmployee GetManager(IDepartment department)
    {
      return this.GetManager(department, null);
    }
    
    /// <summary>
    /// Получить сотрудников подразделения.
    /// </summary>
    /// <param name="department">Подразделение.</param>
    /// <returns>Список сотрудников.</returns>
    [Public]
    public virtual List<IRecipient> GetDepartmentRecipients(IDepartment department)
    {
      var recipientList = new List<IRecipient>();
      recipientList.AddRange(department.RecipientLinks.Select(l => l.Member));
      var subDepartments = Departments.GetAll(l => Equals(l.HeadOffice, department));
      if (subDepartments.Any())
        foreach (var subDepartment in subDepartments)
          recipientList.AddRange(this.GetDepartmentRecipients(subDepartment));
      return recipientList;
    }
    
    /// <summary>
    /// Получить реципиентов по организации.
    /// </summary>
    /// <param name="businessUnit">Организация.</param>
    /// <returns>Активные реципиенты организации и активные роли.</returns>
    [Remote]
    public List<IRecipient> GetFiltredRecipients(IBusinessUnit businessUnit)
    {
      var businessUnitRecipients = Recipients.GetAll()
        .Where(r => r.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
        .AsEnumerable()
        .Where(r => r.IncludedIn(businessUnit));
      
      var roles = Roles
        .GetAll()
        .Where(r => r.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
        .AsEnumerable()
        .Select(r => Recipients.As(r));
      return roles.Union(businessUnitRecipients).OrderBy(r => r.Name).ToList();
    }
    
    #endregion
    
    #region Функции для работы с настройками задач.
    
    /// <summary>
    /// Обрезать инструкцию до 500 символов.
    /// </summary>
    /// <param name="instruction">Инструкция.</param>
    /// <returns>Если длина инструкции не более 500 символов, то вернуть исходную строку. Если длина инструкции более 500 символов, то обрезать до 500 символов.</returns>
    [Remote, Public]
    public string TrimInstruction(string instruction)
    {
      return instruction.Length > 500 ? instruction.Substring(0, 500) : instruction;
    }
    
    /// <summary>
    /// Найти настройку задачи.
    /// </summary>
    /// <param name="taskType">Тип задачи. Пример заполнения: HRLite.AcquaintanceTasks.Info.Name.</param>
    /// <param name="documentKind">Вид докуента.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Настройка задачи.</returns>
    [Public, Remote]
    public ITaskSetting GetTaskSetting(string taskType, IDocumentKind documentKind, IBusinessUnit businessUnit)
    {
      // Отфильтовать изначально по Типу задачи, сразу убрать из подборки настройки по другим НОР и по другим Видам, а также закрытые записи.
      var taskSettings = TaskSettings.GetAll(s => s.TaskType == taskType && (Equals(s.DocumentKind, documentKind) || s.DocumentKind == null) &&
                                             (Equals(s.BusinessUnit, businessUnit) || s.BusinessUnit == null) && s.Status == HRManagement.TaskSetting.Status.Active);
      if (!taskSettings.Any())
        return null;
      
      // Отфильтовать по Виду документа. Если не нашли настройку для нужного вида, искать настройку с пустым видом.
      var taskSettingsWithFilter = taskSettings.Where(s => Equals(s.DocumentKind, documentKind));
      if (taskSettingsWithFilter.Any())
        taskSettings = taskSettingsWithFilter;
      else
        taskSettings = taskSettings.Where(s => s.DocumentKind == null);
      if (!taskSettings.Any())
        return null;
      
      // Далее отфильтовать по Нашей организации. Если не нашли настройку для НОР, искать настройку с пустой НОР.
      taskSettingsWithFilter = taskSettings.Where(s => Equals(s.BusinessUnit, businessUnit));
      if (taskSettingsWithFilter.Any())
        return taskSettingsWithFilter.FirstOrDefault();
      else
        taskSettings = taskSettings.Where(s => s.BusinessUnit == null);
      if (taskSettings.Any())
        return taskSettings.FirstOrDefault();
      
      return null;
    }

    /// <summary>
    /// Найти запись справочника настроек для нашей организации.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Запись справочника настроек.</returns>
    [Remote, Public]
    public IHRSettings GetModuleSettings(IBusinessUnit businessUnit)
    {
      var settings = HRSettingses.GetAll(l => l.BusinessUnit.Equals(businessUnit));
      if (settings.Any())
        return settings.First();
      else
        return HRSettingses.GetAll(l => l.BusinessUnit == null).FirstOrDefault();
    }

    /// <summary>
    /// Проверить, что пользователь входит в роль для отладки.
    /// </summary>
    /// <returns>True, если входит в роль.</returns>
    [Remote, Public]
    public bool IsDebugEnabled()
    {
      return Users.Current.IncludedIn(Constants.Module.HRDebugUsers);
    }
    
    #endregion
    
    #region Отпуска.
    
    /// <summary>
    /// Создать запись справочника Отпуска.
    /// </summary>
    /// <param name="vacationDates">Даты отпуска.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <param name="status">Статус отпуска.</param>
    [Remote]
    public void CreateVacation(Structures.Vacation.IVacationDates vacationDates, IEmployee employee, DateTime year, Sungero.Core.Enumeration status)
    {
      var vacation = Vacations.Create();
      vacation.Employee = employee;
      vacation.StartDate = vacationDates.BeginDate;
      vacation.VacationDuration = vacationDates.Duration;
      vacation.VacationKind = vacationDates.VacationKind;
      vacation.Status = status;
      vacation.Year = year;
      vacation.Save();
    }
    
    /// <summary>
    /// Создать актуальный график отпусков.
    /// </summary>
    /// <param name="businessUnit">Наши организации.</param>
    /// <param name="selectedDepartments">Подразделения.</param>
    /// <param name="selectedEmployees">Сотрудники.</param>
    /// <param name="onliyPaid">True - если выводить только оплачиваемые.</param>
    /// <param name="year">Год.</param>
    /// <returns>Актуальный график отпусков.</returns>
    [Remote]
    public virtual ISimpleDocument BuildActualVacationReportDocument(IBusinessUnit businessUnit,
                                                                     List<IDepartment> selectedDepartments,
                                                                     List<IEmployee> selectedEmployees,
                                                                     bool onliyPaid,
                                                                     DateTime year)
    {
      var vacations = Vacations.GetAll(v => v.Year.Value.Year == year.Year &&
                                       Equals(v.BusinessUnit, businessUnit));
      if (selectedDepartments.Any())
        vacations = vacations.Where(l => selectedDepartments.Contains(l.Department));
      if (selectedEmployees.Any())
        vacations = vacations.Where(l => selectedEmployees.Contains(l.Employee));
      if (onliyPaid)
        vacations = vacations.Where(l => l.VacationKind.Paid == true);
      
      var workbook = new Workbook();
      workbook.Worksheets.Clear();

      // Задать стиль по умолчанию.
      Style wbStyle = workbook.CreateStyle();
      wbStyle.Font.Name = "Arial";
      wbStyle.Font.Size = 10;
      wbStyle.HorizontalAlignment = TextAlignmentType.Center;
      wbStyle.VerticalAlignment = TextAlignmentType.Center;
      workbook.DefaultStyle = wbStyle;
      Worksheet worksheet = workbook.Worksheets.Add(Vacations.Resources.ActualVacationSheetName);
      worksheet.PageSetup.Orientation = PageOrientationType.Landscape;

      #region Стили
      // Стиль для отображения названий подразделений.
      var deparmentStyle = workbook.CreateStyle();
      deparmentStyle.Font.IsBold = true;
      deparmentStyle.HorizontalAlignment = TextAlignmentType.Center;
      deparmentStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
      deparmentStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
      
      // Стиль для отображения ячеек с данными.
      var cellStyle = workbook.CreateStyle();
      cellStyle.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
      cellStyle.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
      cellStyle.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
      cellStyle.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
      cellStyle.IsTextWrapped = true;
      
      // Стиль для отображения ячеек с заголовком отчета.
      var headerStyle = workbook.CreateStyle();
      headerStyle.Font.IsBold = true;
      #endregion
      
      // Вписать во вторую строку название отчета.
      worksheet.Cells[1, 0].Value = Resources.ActualReportHeader;
      worksheet.Cells[2, 0].Value = Resources.ActualReportHeaderDataFormat(businessUnit.Name, year.Year, Calendar.Today.ToShortDateString());
      worksheet.Cells.CreateRange(1, 0, 2, 6).RowHeight = 25.5;
      worksheet.Cells.CreateRange(1, 0, 2, 6).SetStyle(headerStyle);
      worksheet.Cells.CreateRange(1, 0, 1, 6).Merge();
      worksheet.Cells.CreateRange(2, 0, 1, 6).Merge();
      // Заголовок таблицы.
      worksheet.Cells[4, 0].Value = Resources.EmployeeFieldName;
      worksheet.Cells[4, 1].Value = DirRX.HRManagement.Resources.ReportVacationKind;
      worksheet.Cells[4, 2].Value = Resources.ReportDateBegin;
      worksheet.Cells[4, 3].Value = Resources.ReportDateEnd;
      worksheet.Cells[4, 4].Value = Resources.ReportStatus;
      worksheet.Cells[4, 5].Value = Resources.ReportBaseDocs;
      worksheet.Cells.CreateRange(0, 0, 1, 1).ColumnWidth = 24.5;
      worksheet.Cells.CreateRange(0, 1, 1, 4).ColumnWidth = 14;
      worksheet.Cells.CreateRange(0, 5, 1, 1).ColumnWidth = 40;
      worksheet.Cells.CreateRange(4, 0, 1, 6).SetStyle(cellStyle);
      // Закрепить область заглоловка.
      worksheet.FreezePanes(5, 0, 5, 6);
      
      var departments = vacations.OrderBy(l => l.Employee.Department.Name).Select(l => l.Employee.Department).Distinct();
      var rowIndex = 5;
      foreach (var departmentSet in departments)
      {
        // Заполнить ячейку с названием подразделения.
        worksheet.Cells[rowIndex, 0].Value = departmentSet.Name;
        worksheet.Cells.CreateRange(rowIndex, 0, 1, 6).Merge();
        worksheet.Cells.CreateRange(rowIndex, 0, 1, 6).SetStyle(deparmentStyle);
        var employees = vacations.Where(l => Equals(l.Department, departmentSet))
          .OrderBy(l => l.Employee.Name)
          .Select(l => l.Employee)
          .Distinct();
        foreach (var employeeRow in employees)
        {
          var employeeVacations = vacations.Where(l => Equals(l.Department, departmentSet) && Equals(l.Employee, employeeRow))
            .OrderBy(x => x.Employee.Name)
            .ThenBy(x => x.StartDate);
          var employeeFirstRow = rowIndex + 1;
          foreach (var singleVacation in employeeVacations)
          {
            // Заполнить ячейки с данными по отпуску.
            rowIndex++;
            worksheet.Cells[rowIndex, 1].Value = singleVacation.VacationKind.Name;
            worksheet.Cells[rowIndex, 2].Value = singleVacation.StartDate.Value.ToShortDateString();
            worksheet.Cells[rowIndex, 3].Value = singleVacation.FinDate.Value.ToShortDateString();
            worksheet.Cells[rowIndex, 4].Value = Vacations.Info.Properties.Status.GetLocalizedValue(singleVacation.Status.Value);
            worksheet.Cells[rowIndex, 5].SetStyle(cellStyle);
            var bindDocs = Functions.Vacation.GetAllDocumentsForVacation(singleVacation);
            if (bindDocs.Any())
            {
              foreach (var document in bindDocs)
              {
                // Заполнить ячейки с ссылками на документ.
                worksheet.Cells[rowIndex, 5].SetStyle(cellStyle);
                worksheet.Cells[rowIndex, 5].Value = document.Name;
                worksheet.Hyperlinks.Add(rowIndex, 5, 1, 1, Hyperlinks.Get(document));
                rowIndex++;
              }
              // Объединить ячейки с данными по отпуску.
              var docsCount = bindDocs.Count;
              worksheet.Cells.CreateRange(rowIndex - docsCount, 1, docsCount, 1).Merge();
              worksheet.Cells.CreateRange(rowIndex - docsCount, 2, docsCount, 1).Merge();
              worksheet.Cells.CreateRange(rowIndex - docsCount, 3, docsCount, 1).Merge();
              rowIndex--;
            }
          }
          // Объединить ячейки с именем сотрудника.
          worksheet.Cells[employeeFirstRow, 0].Value = employeeRow.Name;
          worksheet.Cells.CreateRange(employeeFirstRow, 0, rowIndex - employeeFirstRow + 1, 1).Merge();
          worksheet.Cells.CreateRange(employeeFirstRow, 0, rowIndex - employeeFirstRow + 1, 5).SetStyle(cellStyle);
        }
        rowIndex++;
      }
      
      worksheet.Cells.CreateRange(4, 0, rowIndex, 1).RowHeight = 25.5;
      
      // Создать документ.
      var report = SimpleDocuments.Create();
      report.Name = Vacations.Resources.ActualVacationReportNameFormat(businessUnit.Name, year.Year);

      // Перенести тело документа Aspose в новую версию документа приложения.
      using (var memory = new System.IO.MemoryStream())
      {
        workbook.Save(memory, SaveFormat.Xlsx);
        report.CreateVersionFrom(memory, "xlsx");
      }
      report.Save();
      return report;
    }
    
    /// <summary>
    /// Получить спиcок продолжительностей отпусков, которые не распланированы полностью.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <returns>Список продолжительностей отпусков.</returns>
    [Remote]
    public virtual List<IVacationDurationAvailableDuration> GetEmployeeUnScheduledDurations(IEmployee employee, DateTime year)
    {
      var employeeDurations = Functions.Module.GetEmployeeVacationDuration(employee, year);
      var planedVacations = Vacations.GetAll(v => Equals(v.Employee, employee) &&
                                             v.Year.Value.Year == year.Year &&
                                             !(v.Status == DirRX.HRManagement.Vacation.Status.Shifted || v.Status == DirRX.HRManagement.Vacation.Status.Canceled));
      var unScheduledDurations = new List<IVacationDurationAvailableDuration>();
      if (employeeDurations != null)
      {
        foreach (var duration in employeeDurations.AvailableDuration)
        {
          var planedVacationsByKind = planedVacations.Where(l => Equals(l.VacationKind, duration.VacationKind));
          var amountOfDays = planedVacationsByKind.Any() ? planedVacationsByKind.Sum(l => l.VacationDuration.Value) : 0;
          if (duration.DaysCount.Value > amountOfDays)
            unScheduledDurations.Add(duration);
        }
      }
      return unScheduledDurations;
    }
    
    /// <summary>
    /// Проверить, что в подразделении идет процесс планирования отпусков.
    /// </summary>
    /// <param name="department">Подразделение.</param>
    /// <returns>True, если в подразделении идет процесс планирования отпусков.</returns>
    [Remote(IsPure = true), Public]
    public bool IsVacationSchedulingInDepartment(IDepartment department)
    {
      return Functions.Module.GetDocflowParamValue(string.Format(ParamKeys.SchedulingKeyDepartment, department.Id)).Equals(true.ToString());
    }
    
    /// <summary>
    /// Проверить, что в подразделении пользователя идет процесс планирования отпусков.
    /// </summary>
    /// <param name="users">Список работников.</param>
    /// <returns>True, если в подразделении хотя бы одного из переданных пользователей идет процесс планирования отпусков.</returns>
    [Remote(IsPure = true), Public]
    public bool IsVacationSchedulingInDepartment(List<IUser> users)
    {
      foreach (var user in users)
      {
        var employee = Employees.As(user);
        if (this.IsVacationSchedulingInDepartment(employee.Department))
          return true;
      }
      return false;
    }
    
    /// <summary>
    /// Проверить, что в организации идет процесс планирования отпусков.
    /// </summary>
    /// <param name="businessUnit">Организация.</param>
    /// <returns>True, если в организации идет процесс планирования отпусков.</returns>
    [Remote(IsPure = true), Public]
    public bool IsVacationSchedulingInBusinessUnit(IBusinessUnit businessUnit)
    {
      return Functions.Module.GetDocflowParamValue(string.Format(ParamKeys.SchedulingKeyBusinessUnit, businessUnit.Id)).Equals(true.ToString());
    }
    
    /// <summary>
    /// Проверить, что в организации пользователя идет процесс планирования отпусков.
    /// </summary>
    /// <param name="users">Список работников.</param>
    /// <returns>True, если в организации хотя бы одного из переданных пользователей идет процесс планирования отпусков.</returns>
    [Remote(IsPure = true), Public]
    public bool IsVacationSchedulingInBusinessUnit(List<IUser> users)
    {
      foreach (var user in users)
      {
        var employee = Employees.As(user);
        if (employee != null)
        {
          var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
          if (this.IsVacationSchedulingInBusinessUnit(businessUnit))
            return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Создать задачу на планирование отпуска по сотруднику.
    /// </summary>
    /// <returns>Задача на планирование.</returns>
    [Remote]
    public virtual IVacationSingleSchedulingTask CreateSingleSchedulingTask()
    {
      return VacationSingleSchedulingTasks.Create();
    }
    
    /// <summary>
    /// Измененить cостояния отпусков.
    /// </summary>
    /// <param name="vacations">Отпуска.</param>
    /// <param name="status">Состояние.</param>
    public virtual void VacationSwitchStatus(IQueryable<IVacation> vacations, Enumeration status)
    {
      foreach (var vacation in vacations)
      {
        var lockInfo = Locks.GetLockInfo(vacation);
        if (lockInfo.IsLocked)
          continue;
        vacation.Status = status;
        vacation.Save();
      }
    }
    
    /// <summary>
    /// Проверить, является ли день платежным.
    /// </summary>
    /// <param name="day">День.</param>
    /// <returns>True, если день платежный.</returns>
    [Remote]
    public virtual bool IsPaymentDay(DateTime day)
    {
      return Calendar.IsWorkingDay(day);
    }
    
    /// <summary>
    /// Получить ближайший платежный день перед датой.
    /// </summary>
    /// <param name="businessUnit">НОР.</param>
    /// <param name="date">Дата.</param>
    /// <returns>Платежный день.</returns>
    [Public]
    public virtual DateTime GetPaymentDayBeforeDate(IBusinessUnit businessUnit, DateTime date)
    {
      var paymentCalendar = PaymentCalendars.GetAll(p => (p.BusinessUnits.Any(b => b.BusinessUnit.Equals(businessUnit)) || !p.BusinessUnits.Any()) &&
                                                    p.Year.Value.Year <= date.Year);
      
      var paymentDay = date;
      if (paymentCalendar.Any())
        paymentDay = paymentCalendar.OrderByDescending(o => o.Year.Value)
          .Select(v => v.Calendar.Where(c => c.IsPayment == true && date >= c.Date)
                  .OrderByDescending(o => o.Date)
                  .Select(s => s.Date.Value)
                  .FirstOrDefault())
          .FirstOrDefault();
      
      return paymentDay;
    }
    
    /// <summary>
    /// Получить ближайший платежный день после даты.
    /// </summary>
    /// <param name="businessUnit">НОР.</param>
    /// <param name="date">Дата.</param>
    /// <returns>Платежный день.</returns>
    [Public]
    public virtual DateTime GetPaymentDayAfterDate(IBusinessUnit businessUnit, DateTime date)
    {
      var paymentCalendar = PaymentCalendars.GetAll(p => (p.BusinessUnits.Any(b => b.BusinessUnit.Equals(businessUnit)) || !p.BusinessUnits.Any()) &&
                                                    p.Year.Value.Year <= date.Year);
      
      var paymentDay = date;
      if (paymentCalendar.Any())
        paymentDay = paymentCalendar.OrderByDescending(o => o.Year.Value)
          .Select(v => v.Calendar.Where(c => c.IsPayment == true && date <= c.Date)
                  .OrderBy(o => o.Date)
                  .Select(s => s.Date.Value)
                  .FirstOrDefault())
          .FirstOrDefault();
      
      return paymentDay;
    }
    
    /// <summary>
    /// Получить вид отпуска.
    /// </summary>
    /// <param name="sid">Идентификатор.</param>
    /// <returns>Вид отпуска.</returns>
    [Remote]
    public virtual IVacationKind GetVacationKind(string sid)
    {
      return VacationKinds.GetAll(k => k.Sid == sid).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить основной оплачиваемый вид отпуска.
    /// </summary>
    /// <param name="recipient">Сотрудник.</param>
    /// <returns>Вид отпуска.</returns>
    [Public, Remote]
    public virtual IVacationKind GetPaidVacationKind(IRecipient recipient)
    {
      var businessUnit = Employees.As(recipient).Department.BusinessUnit;
      var settings = Functions.Module.GetModuleSettings(businessUnit);
      return settings.MainVacationKind;
    }
    
    /// <summary>
    /// Получить вид отпуска без сохранения з/п.
    /// </summary>
    /// <returns>Вид отпуска.</returns>
    [Public, Remote]
    public virtual IVacationKind GetVacationWithoutPayKind()
    {
      return VacationKinds.GetAll(k => k.Sid == Constants.VacationKind.WithoutPay).FirstOrDefault();
    }
    
    /// <summary>
    /// Создать замещение.
    /// </summary>
    /// <param name="who">Замещаемый.</param>
    /// <param name="byWhom">Замещающий.</param>
    /// <param name="fromDate">Дата начала.</param>
    /// <param name="toDate">Дата окончания.</param>
    /// <param name="comment">Описание.</param>
    /// <returns>Замещение.</returns>
    public ISubstitution CreateSubstitution(IUser who, IUser byWhom, DateTime fromDate, DateTime toDate, string comment)
    {
      var substitution = Substitutions.Create();
      substitution.User = who;
      substitution.Substitute = byWhom;
      substitution.StartDate = fromDate;
      substitution.EndDate = toDate;
      substitution.Comment = comment;
      substitution.Save();
      return substitution;
    }
    
    /// <summary>
    /// Получить индивидуальную продолжительность отпуска для сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <returns>Продолжительность отпуска.</returns>
    [Public, Remote]
    public virtual IVacationDuration GetEmployeeVacationDuration(IEmployee employee, DateTime year)
    {
      return VacationDurations.GetAll(l => Equals(l.Employee, employee) && l.Year.Value.Year == year.Year).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить задачу на подтверждение отпуска.
    /// </summary>
    /// <param name="vacation">Отпуск.</param>
    /// <returns>Задача.</returns>
    public virtual Sungero.Workflow.ITask GetVacationAlertTask(IVacation vacation)
    {
      return VacationAlertTasks.GetAll(a => a.Vacation.Equals(vacation)).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить отпуска, по которым необходимо отправить задачи на подтверждение.
    /// </summary>
    /// <returns>Список отпусков.</returns>
    public virtual List<IVacation> GetVacationForAlert()
    {
      var vacationsForAlert = new List<IVacation>();
      var today = Calendar.Today;
      var allBusinessUnits = BusinessUnits.GetAll();
      var allSettings = HRSettingses.GetAll();
      var settingsBusinessUnits = allSettings.Where(l => l.BusinessUnit != null).Select(l => l.BusinessUnit);
      foreach (var settings in allSettings)
      {
        var businessUnit = settings.BusinessUnit;
        var daysBeforeAlert = settings.DaysCountBeforeVacationAlert.Value;
        var monitoringDate = today.AddDays(daysBeforeAlert);
        var vacations = Vacations.GetAll(v => v.NoteDate == null &&
                                         v.StartDate <= monitoringDate &&
                                         v.Status == DirRX.HRManagement.Vacation.Status.Active &&
                                         (v.BusinessUnit.Equals(businessUnit) || (!settingsBusinessUnits.Contains(v.BusinessUnit) && businessUnit == null)) &&
                                         v.Employee.Status == Sungero.Company.Employee.Status.Active);
        var employees = vacations.Select(v => v.Employee).Distinct();
        foreach (var employee in employees)
          vacationsForAlert.Add(vacations.Where(v => v.Employee.Equals(employee)).OrderBy(v => v.StartDate).First());
      }
      return vacationsForAlert;
    }
    
    /// <summary>
    /// Выполнить задание на подтверждение дат опуска.
    /// </summary>
    /// <param name="task">Задача.</param>
    public virtual void CompleteAlertAssignment(Sungero.Workflow.ITask task)
    {
      var shiftTask = VacationShiftTasks.As(task);
      var appAssignment = VacationApprovalAssignments.As(shiftTask.ApprovalAssignment);
      appAssignment.ActiveText = string.Format("{0}.", VacationApprovalAssignments.Info.Actions.StartShiftTask.LocalizedName);
      appAssignment.ShiftTask = shiftTask;
      appAssignment.Complete(HRManagement.VacationApprovalAssignment.Result.Shift);
    }
    
    /// <summary>
    /// Создать и отправить задачу на подтверждение опуска. Если две порции идут подряд (т.е. между порциями нет рабочих дней), то они присылаются одним заданием.
    /// </summary>
    /// <param name="vacation">Отпуск.</param>
    [Remote]
    public virtual void CreateAndStartAlertTask(IVacation vacation)
    {
      var task = this.CreateNewVacationAlertTask();
      var alertTask = VacationAlertTasks.As(task);
      alertTask.Vacation = vacation;
      var employee = vacation.Employee;
      alertTask.EmployeeAttachmentGroup.Employees.Add(employee);
      var employeeShortName = Sungero.Company.PublicFunctions.Employee.GetShortName(employee, Sungero.Core.DeclensionCase.Genitive, false);
      while (vacation != null)
      {
        alertTask.Vacations.AddNew().Vacation = vacation;
        var lockInfo = Locks.GetLockInfo(vacation);
        if (!lockInfo.IsLocked)
        {
          vacation.NoteDate = Calendar.Today;
          vacation.Save();
        }
        var vacationList = Vacations.GetAll(v => v.NoteDate == null &&
                                            v.StartDate > vacation.FinDate.Value &&
                                            v.StartDate <= Calendar.NextWorkingDay(vacation.FinDate.Value) &&
                                            v.Status == DirRX.HRManagement.Vacation.Status.Active &&
                                            (v.Employee == employee));
        vacation = vacationList.Any() ? vacationList.OrderBy(v => v.StartDate).First() : Vacations.Null;
      }
      alertTask.Subject = VacationAlertTasks.Resources.VacationAlertTaskSubjectFormat(employeeShortName,
                                                                                      alertTask.Vacations.OrderBy(v => v.Vacation.StartDate).First().Vacation.StartDate.Value.ToShortDateString(),
                                                                                      alertTask.Vacations.OrderByDescending(v => v.Vacation.FinDate).First().Vacation.FinDate.Value.ToShortDateString());
      alertTask.Start();
    }
    
    /// <summary>
    /// Прекратить задачу на перенос отпуска.
    /// </summary>
    /// <param name="task">Задача на перенос.</param>
    [Public]
    public virtual void StopVacationShiftTask(Sungero.Workflow.ITask task)
    {
      var shiftTask = VacationShiftTasks.As(task);
      if (task != null)
      {
        shiftTask.AbortingReason = VacationAlertTasks.Resources.ShiftTaskAbortingReasons;
        shiftTask.Save();
        shiftTask.Abort();
      }
    }

    /// <summary>
    /// Проверить, что состоялся перенос отпуска.
    /// </summary>
    /// <param name="task">Задача на перенос.</param>
    /// <returns>True, если подписан приказ о переносе.</returns>
    [Public]
    public virtual bool VacationShiftIsComplete(Sungero.Workflow.ITask task)
    {
      var shiftTask = VacationShiftTasks.As(task);
      return ShiftSignDocsAssignments.GetAll(v => Equals(v.Task, shiftTask) && Equals(v.Result.Value, HRManagement.ShiftSignDocsAssignment.Result.Complete) &&
                                             v.BlockUid == HRManagement.Constants.VacationShiftTask.SignSeoDocAssignmentBlockId).Any();
    }
    
    /// <summary>
    /// Проверить, что перенос отпуска был отменен.
    /// </summary>
    /// <param name="task">Задача на перенос.</param>
    /// <returns>True, если перенос был отменен.</returns>
    [Public]
    public virtual bool VacationShiftIsCancel(Sungero.Workflow.ITask task)
    {
      var shiftTask = VacationShiftTasks.As(task);
      var isShiftTaskStop = ShiftStatementApprovalAssignments.GetAll(v => Equals(v.Task, task) && Equals(v.Result.Value, HRManagement.ShiftStatementApprovalAssignment.Result.Stop)).Any();
      var isShiftTaskAborted = task.Status == HRManagement.VacationShiftTask.Status.Aborted;
      
      return isShiftTaskStop || isShiftTaskAborted;
    }
    
    /// <summary>
    /// Создать задачу на перенос отпуска.
    /// </summary>
    /// <param name="vacations">Список отпусков.</param>
    /// <param name="assignment">Задание из которого стартует перенос.</param>
    /// <returns>Задача на перенос отпуска.</returns>
    [Public, Remote]
    public virtual Sungero.Workflow.ITask CreateNewVacationShiftTask(List<IVacation> vacations, Sungero.Workflow.IAssignment assignment)
    {
      var shiftTask = VacationShiftTasks.As(this.CreateNewVacationShiftTask());
      shiftTask.ApprovalAssignment = VacationApprovalAssignments.As(assignment);
      var firstVacation = vacations.First();
      shiftTask.Employee = firstVacation.Employee;
      shiftTask.Vacation1 = firstVacation;
      if (vacations.Count >= 2)
        shiftTask.Vacation2 = vacations.ElementAt(1);
      if (vacations.Count == 3)
        shiftTask.Vacation3 = vacations.ElementAt(2);
      return shiftTask;
    }
    
    /// <summary>
    /// Сформировать лист ознакомления с графиком отпусков по подразделению.
    /// </summary>
    /// <param name="department">Подразделение.</param>
    /// <param name="year">Год.</param>
    /// <param name="reportDate">Дата документа графика отпусков.</param>
    /// <param name="reportNumber">Номер документа графика отпусков.</param>
    /// <returns>Лист ознакомления.</returns>
    [Remote, Public]
    public virtual IHRDocument CreateNewVacationAcquaintanceList(IDepartment department, int year, DateTime reportDate, string reportNumber)
    {
      var document = HRManagement.HRDocuments.Create();
      var report = Reports.GetVacationAcquaintanceList();
      report.Year = year;
      report.Department = department;
      report.VacationReportDate = reportDate.ToShortDateString();
      report.VacationReportNumber = reportNumber;
      var internalReport = (Sungero.Reporting.Shared.IInternalReport)report;
      if (internalReport == null)
        return null;
      
      using (var reportStream = new MemoryStream())
      {
        internalReport.InternalExecute(reportStream);
        document.CreateVersionFrom(reportStream, "pdf");
        document.LastVersion.Author = Users.Current;
        document.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
        document.AssociatedApplication = Sungero.Content.AssociatedApplications.GetAll(app => app.Extension == "pdf").FirstOrDefault();
        document.DocumentKind = DocKindFunctions.GetNativeDocumentKind(DocKind.VacationScheduleAcquaintanceListKind);
        document.Department = department;
        document.Employee = Employees.GetAll(l => l.Department.Equals(department)).FirstOrDefault();
        document.BusinessUnit = department.BusinessUnit;
        document.Year = new DateTime(year, 1, 1);
        document.State.Properties.AssociatedApplication.IsRequired = false;
        document.Save();
      }
      document.LifeCycleState = HRManagement.HRDocument.LifeCycleState.Active;
      return document;
    }
    
    /// <summary>
    /// Создать задачу на подтверждение дат отпуска.
    /// </summary>
    /// <returns>Созданная задача на подтверждение дат отпуска.</returns>
    [Remote(PackResultEntityEagerly = true)]
    public virtual Sungero.Workflow.ITask CreateNewVacationAlertTask()
    {
      return VacationAlertTasks.Create();
    }

    /// <summary>
    /// Создать задачу на перенос отпуска.
    /// </summary>
    /// <returns>Созданная задача на перенос отпуска.</returns>
    [Public, Remote(PackResultEntityEagerly = true)]
    public virtual Sungero.Workflow.ITask CreateNewVacationShiftTask()
    {
      return VacationShiftTasks.Create();
    }

    /// <summary>
    /// Создать задачу на согласование заявления на отпуск.
    /// </summary>
    /// <returns>Созданная задача на согласование отпуска.</returns>
    [Remote(PackResultEntityEagerly = true)]
    public virtual Sungero.Workflow.ITask CreateNewVacationApprovalTask()
    {
      return VacationApprovalTasks.Create();
    }

    /// <summary>
    /// Создать задачу на увольнение.
    /// </summary>
    /// <returns>Созданная задача на увольнение.</returns>
    [Remote(PackResultEntityEagerly = true)]
    public DirRX.HRManagement.IDismissalTask CreateNewDismissalTask()
    {
      return DismissalTasks.Create();
    }

    /// <summary>
    /// Создать задачу на перевод.
    /// </summary>
    /// <returns>Созданная задача на перевод.</returns>
    [Remote(PackResultEntityEagerly = true)]
    public DirRX.HRManagement.ITransferTask CreateNewTransferTask()
    {
      return TransferTasks.Create();
    }
    
    #endregion
    
    #region Отчет для проверки настроек.
    /// <summary>
    /// Подготовить данные для добавления в отчет о настройке.
    /// </summary>
    /// <param name="title">Заголовок проверки.</param>
    /// <param name="data">Данные.</param>
    /// <returns>Строка с информацией о данных.</returns>
    private string FormatInfoForSettingReport(string title, IQueryable data)
    {
      var dataInfo = title + Environment.NewLine;
      foreach (var item in data)
        dataInfo += "\t" + item.ToString() + Environment.NewLine;
      dataInfo += Environment.NewLine;
      return dataInfo;
    }
    
    /// <summary>
    /// Подготовить данные для добавления в отчет о настройке.
    /// </summary>
    /// <param name="title">Заголовок проверки.</param>
    /// <param name="data">Данные.</param>
    /// <returns>Строка с информацией о данных.</returns>
    private string FormatInfoForSettingReport(string title, List<string> data)
    {
      var dataInfo = title + Environment.NewLine;
      foreach (var item in data)
        dataInfo += "\t" + item + Environment.NewLine;
      dataInfo += Environment.NewLine;
      return dataInfo;
    }
    
    /// <summary>
    /// Построить отчет для проверки настроек системы.
    /// </summary>
    /// <returns>Документ с отчетом.</returns>
    [Public, Remote]
    public ISimpleDocument BuildSettingsReport()
    {
      var reportText = string.Empty;
      
      #region Выбрать подразделения без руководителя.
      var departments = Departments.GetAll(l => l.Manager == null && l.Status == Sungero.Company.Department.Status.Active);
      if (departments.Any())
        reportText += this.FormatInfoForSettingReport(Resources.DepartmentsWithoutManagers, departments);
      #endregion
      
      #region Проверить подразделения без НОР.
      var departmentsWOBusinessUnit = Departments.GetAll(l => l.BusinessUnit == null && l.Status == Sungero.Company.Department.Status.Active);
      if (departmentsWOBusinessUnit.Any())
        reportText += this.FormatInfoForSettingReport(Resources.DepartmentsWithoutBusinessUnit, departmentsWOBusinessUnit);
      #endregion
      
      #region Выбрать неавтоматизированных пользователей.
      var users = Users.GetAll(l => l.Login == null && l.Status == Sungero.CoreEntities.User.Status.Active);
      if (users.Any())
        reportText += this.FormatInfoForSettingReport(Resources.UsersWithoutLogins, users);
      #endregion

      #region Проверить неавтоматизированных пользователей в ролях
      var roles = Roles.GetAll(r => (r.Sid == HRRoles.ITResponsible ||
                                     r.Sid == DirRX.HRLite.PublicConstants.Module.Roles.HRSpecialist ||
                                     r.Sid == HRRoles.SecurityResponsible ||
                                     r.Sid == HRRoles.SupplyResponsible ||
                                     r.Sid == HRRoles.StaffChief ||
                                     r.Sid == HRRoles.Accounter ||
                                     r.Sid == HRRoles.VacationManagers ||
                                     r.Sid == HRRoles.HRSettingsManager ||
                                     r.Sid == HRRoles.VacationResponsible) && r.RecipientLinks.Where(x => Employees.Is(x.Member) && Employees.As(x.Member).Login == null).Any());
      
      if (roles.Any())
        reportText += this.FormatInfoForSettingReport(Resources.UsersWithoutLoginsInRoles, roles);
      #endregion
      
      #region Проверить исполнителей ролей по каждой НОР
      foreach (IBusinessUnit bu in BusinessUnits.GetAll()) {
        var reciepients = new List<string>();
        if (this.GetSupplyResponsible(bu) == null)
          reciepients.Add(Functions.Module.GetRole(HRRoles.SupplyResponsible).Name);
        if (this.GetSecurityResponsible(bu) == null)
          reciepients.Add(Functions.Module.GetRole(HRRoles.SecurityResponsible).Name);
        if (this.GetITResponsible(bu) == null)
          reciepients.Add(Functions.Module.GetRole(HRRoles.ITResponsible).Name);
        if (this.GetVacationResponsible(bu) == null)
          reciepients.Add(Functions.Module.GetRole(HRRoles.VacationResponsible).Name);
        if (this.GetAccounter(bu) == null)
          reciepients.Add(Functions.Module.GetRole(HRRoles.Accounter).Name);
        if (this.GetStaffChief(bu) == null)
          reciepients.Add(Functions.Module.GetRole(HRRoles.StaffChief).Name);
        if (this.GetHRResponsible(bu) == null)
          reciepients.Add(Functions.Module.GetRole(DirRX.HRLite.PublicConstants.Module.Roles.HRSpecialist).Name);
        if (reciepients.Count() > 0)
          // reportText += FormatInfoForSettingReport(string.Format(Resources.BusinessUnitsRoleWithoutRecipients, bu.Name), reciepients);
          reportText += this.FormatInfoForSettingReport(Resources.BusinessUnitsRoleWithoutRecipientsFormat(bu.Name), reciepients);
      }
      
      #endregion

      // Создать документ.
      if (string.IsNullOrEmpty(reportText))
        return null;
      else
      {
        var report = SimpleDocuments.Create();
        report.Name = Resources.SettingsReportNameFormat(Calendar.Now);
        var reportTextBytes = System.Text.Encoding.UTF8.GetBytes(reportText);
        // Перенести информацию о проверках в новую версию документа.
        using (var memory = new System.IO.MemoryStream(reportTextBytes))
          report.CreateVersionFrom(memory, "txt");
        report.Save();
        return report;
      }
    }
    #endregion
    
    #region Отчет по пересечениям.
    
    /// <summary>
    /// Построить отчет по пересечению отпусков сотрудников.
    /// </summary>
    /// <param name="vacations">Список отпусков.</param>
    /// <param name="year">Год.</param>
    /// <returns>Документ с отчетом.</returns>
    [Public, Remote]
    public ISimpleDocument BuildApproveVacationScheduleByYear(List<IVacation> vacations, DateTime year)
    {
      var workbook = new Workbook();
      workbook.Worksheets.Clear();
      
      this.AddPlanningVacationsSheetInSchedule(workbook, vacations, year);
      
      // Создать документ.
      var report = SimpleDocuments.Create();
      report.Name = Vacations.Resources.OverlappingVacationReportNameFormat(year.Year);

      // Перенести тело документа Aspose в новую версию документа приложения.
      using (var memory = new System.IO.MemoryStream())
      {
        workbook.Save(memory, SaveFormat.Xlsx);
        report.CreateVersionFrom(memory, "xlsx");
      }
      report.Save();
      return report;
    }
    
    /// <summary>
    /// Добавить лист с отпусками в график отпусков.
    /// </summary>
    /// <param name="workbook">Книга с графиком отпусков.</param>
    /// <param name="vacations">Отпуска.</param>
    /// <param name="year">Год.</param>
    public void AddPlanningVacationsSheetInSchedule(Aspose.Cells.Workbook workbook, List<IVacation> vacations, DateTime year)
    {
      var worksheetName = Vacations.Resources.OverlappingVacationReportNameFormat(year.Year);
      // Задать стиль по умолчанию.
      Style wbStyle = workbook.CreateStyle();
      wbStyle.Font.Name = "Arial";
      wbStyle.Font.Size = 10;
      wbStyle.HorizontalAlignment = TextAlignmentType.Center;
      wbStyle.VerticalAlignment = TextAlignmentType.Center;
      workbook.DefaultStyle = wbStyle;
      
      Worksheet worksheet = workbook.Worksheets.Add(worksheetName);
      
      #region Стили
      // Стиль для отображения руководителей.
      var managerStyle = workbook.CreateStyle();
      managerStyle.ForegroundArgbColor = 10921638;
      managerStyle.Pattern = BackgroundType.Solid;
      managerStyle.HorizontalAlignment = TextAlignmentType.Left;

      // Стиль для отображения некритичных нарушений.
      var warningStyle = workbook.CreateStyle();
      warningStyle.ForegroundArgbColor = -256;
      warningStyle.Pattern = BackgroundType.Solid;
      
      // Стиль для отображения критичных нарушений.
      var errorStyle = workbook.CreateStyle();
      errorStyle.ForegroundArgbColor = 16540258;
      errorStyle.Pattern = BackgroundType.Solid;
      
      // Стиль для отображения текущей даты.
      var currentDateStyle = workbook.CreateStyle();
      currentDateStyle.ForegroundArgbColor = 10673071;
      currentDateStyle.Pattern = BackgroundType.Solid;
      currentDateStyle.RotationAngle = 90;
      
      // Стиль для отображения отпусков из других учетных годов
      var nextYearStyle = workbook.CreateStyle();
      nextYearStyle.ForegroundArgbColor = 12966385;
      nextYearStyle.Pattern = BackgroundType.Solid;
      
      // Стиль с выравниванием по левому краю.
      var leftStyle = workbook.CreateStyle();
      leftStyle.HorizontalAlignment = TextAlignmentType.Left;
      
      // Стиль для отображения названий подразделений.
      var deparmentStyle = workbook.CreateStyle();
      deparmentStyle.IsTextWrapped = true;

      // Стиль для отображения дат вертикально.
      var dateStyle = workbook.CreateStyle();
      dateStyle.RotationAngle = 90;
      #endregion
      
      var lastMondayInVacationYear = year.EndOfYear().BeginningOfWeek();
      var maxFinDate = vacations.Max(q => q.FinDate);
      var lastVacationMonday = maxFinDate.Value.BeginningOfWeek();
      // Если отпуск за указанный год перенесен на следующий год, то указать его в качестве последней даты для построения графика.
      if (lastVacationMonday > lastMondayInVacationYear)
        lastMondayInVacationYear = lastVacationMonday;
      
      // Если последняя неделя отпуска выходит за границы учетного года, указать границу в качестве последней даты для построения графика.
      var borderDateMonday = Functions.Module.GetBorderVacationYear(vacations.FirstOrDefault().Employee.Department.BusinessUnit, year).BeginningOfWeek();
      if (lastMondayInVacationYear > borderDateMonday)
        lastMondayInVacationYear = borderDateMonday;
      
      var vacationWeekBegin = Calendar.GetDate(year.Year, 1, 1).BeginningOfWeek();
      // Собрать начала всех недель в году.
      var counter = 0;
      var vacationBeginOfWeeks = new List<Structures.Vacation.DaysOfWeekBeginning>();
      while (vacationWeekBegin <= lastMondayInVacationYear)
      {
        counter++;
        vacationBeginOfWeeks.Add(Structures.Vacation.DaysOfWeekBeginning.Create(vacationWeekBegin, counter));
        vacationWeekBegin = vacationWeekBegin.AddDays(7);
      }
      var lastWeekNumber = counter;
      
      // Кол-во столбцов: Подразделение + ФИО + Кол-во недель + Всего.
      var columnCount = lastWeekNumber + 3;
      // Вписать в первую строку название отчета.
      worksheet.Cells[0, 0].Value = worksheetName;
      // Объеденить ячейки с названием.
      worksheet.Cells.CreateRange(0, 0, 1, columnCount).Merge();
      // Заголовки.
      worksheet.Cells[2, 0].Value = Vacations.Resources.ColumnHeaderDepartment;
      worksheet.Cells[2, 1].Value = Vacations.Resources.ColumnHeaderFIO;
      worksheet.Cells.CreateRange(0, 1, 1, 1).ColumnWidth = 35;
      worksheet.Cells[2, 2 + lastWeekNumber].Value = Vacations.Resources.ColumnHeaderTotal;

      // Установить ширину столбцов для недель.
      worksheet.Cells.CreateRange(2, 2, 1, lastWeekNumber).ColumnWidth = 2.5;
      // Закрепить область заглоловков.
      worksheet.FreezePanes(3, 0, 3, columnCount);
      
      // Добавить информацию по отпускам сотрудиков.
      var rowNumber = 3;
      // Сгруппировать информацию по подразделениям.
      var departmentName = string.Empty;
      var peoplePercentage = 0;
      foreach (var departmentVacations in vacations.GroupBy(v => v.Employee.Department))
      {
        var departmentEmployeesCount = departmentVacations.Select(v => v.Employee).Distinct().Count();
        
        // Заполнить наименование подразделения.
        var department = departmentVacations.First().Employee.Department;
        var firstEmployeeDepartmentName = department.Name;
        var moduleSettings = Functions.Module.GetModuleSettings(department.BusinessUnit);
        peoplePercentage = moduleSettings.PeoplePercentage.Value;
        if (departmentName != firstEmployeeDepartmentName)
        {
          worksheet.Cells[rowNumber, 0].Value = firstEmployeeDepartmentName;
          worksheet.Cells[rowNumber, 0].SetStyle(deparmentStyle);
          var departmentRange = worksheet.Cells.CreateRange(rowNumber, 0, departmentEmployeesCount, 1);
          departmentRange.ColumnWidth = 20;
          departmentRange.Merge();
          departmentName = firstEmployeeDepartmentName;
        }
        var employeeCounter = 0;
        var firstWeek = vacationBeginOfWeeks.Select(v => v.BeginingDate).Min();
        // Сгруппировать информацию по сотруднику.
        foreach (var groupVacations in departmentVacations.OrderBy(l => l.Employee.Name).GroupBy(v => v.Employee))
        {
          var employeeVacationDaysPlanned = 0;
          var vacationEmployee = groupVacations.First().Employee;
          worksheet.Cells[rowNumber, 1].Value = vacationEmployee.Name;
          if (vacationEmployee.Department.Manager != null)
            if (vacationEmployee.Department.Manager.Equals(vacationEmployee))
              worksheet.Cells[rowNumber, 1].SetStyle(managerStyle);
            else
              worksheet.Cells[rowNumber, 1].SetStyle(leftStyle);
          // Проставить кол-во дней отпуска по неделям.
          foreach (var groupVacation in groupVacations.Where(v => v.StartDate.HasValue && v.FinDate.HasValue).OrderBy(v => v.StartDate))
          {
            // Определить номер недели по первому дню для отображения.
            var vacationBeginOfWeek = groupVacation.StartDate.Value.BeginningOfWeek();
            if (vacationBeginOfWeek < firstWeek)
              vacationBeginOfWeek = firstWeek;
            var weekNumber = vacationBeginOfWeeks.Where(w => w.BeginingDate.Equals(vacationBeginOfWeek)).First().WeekIndex;
            // Расчитать по неделям.
            var beginOfWeek = groupVacation.StartDate.Value;
            if (beginOfWeek < vacationBeginOfWeek)
              beginOfWeek = vacationBeginOfWeek;
            var endOfWeek = beginOfWeek.EndOfWeek();
            
            while (groupVacation.FinDate.Value > endOfWeek && endOfWeek < lastMondayInVacationYear.EndOfWeek())
            {
              var amountWeekDays = this.GetPeriodAmountVacationDays(beginOfWeek, endOfWeek, vacationEmployee);
              worksheet.Cells[rowNumber, 1 + weekNumber].Value = amountWeekDays;
              if (groupVacation.Year.Value.Year == year.Year)
                employeeVacationDaysPlanned += amountWeekDays;
              else
                worksheet.Cells[rowNumber, 1 + weekNumber].SetStyle(nextYearStyle);
              worksheet.Hyperlinks.Add(rowNumber,  1 + weekNumber, 1, 1, Hyperlinks.Get(groupVacation));
              
              beginOfWeek = beginOfWeek.NextWeek();
              endOfWeek = endOfWeek.NextWeek().EndOfWeek();
              weekNumber++;
            }
            var amountLastWeekDays = this.GetPeriodAmountVacationDays(beginOfWeek, groupVacation.FinDate.Value, vacationEmployee);
            if (groupVacation.FinDate.Value > lastMondayInVacationYear.EndOfWeek())
              amountLastWeekDays = this.GetPeriodAmountVacationDays(beginOfWeek, lastMondayInVacationYear.EndOfWeek(), vacationEmployee);
            worksheet.Cells[rowNumber, 1 + weekNumber].Value = amountLastWeekDays;
            
            if (groupVacation.Year.Value.Year == year.Year)
              employeeVacationDaysPlanned += amountLastWeekDays;
            else
              worksheet.Cells[rowNumber, 1 + weekNumber].SetStyle(nextYearStyle);
            
            worksheet.Hyperlinks.Add(rowNumber,  1 + weekNumber, 1, 1, Hyperlinks.Get(groupVacation));
          }
          
          var vacationCheckError = this.CheckDateInReport(vacationEmployee, year);
          var errorString = string.Join(" ", vacationCheckError);
          
          worksheet.Cells[rowNumber, 2 + lastWeekNumber].Value = employeeVacationDaysPlanned;
          if (!string.IsNullOrWhiteSpace(errorString))
          {
            worksheet.Cells[rowNumber, 2 + lastWeekNumber].SetStyle(errorStyle);
            worksheet.Cells[rowNumber, 3 + lastWeekNumber].SetStyle(leftStyle);
            worksheet.Cells[rowNumber, 3 + lastWeekNumber].Value = errorString;
          }
          rowNumber++;
          employeeCounter++;
        }
        
        // Добавить итоги по пересечениям оптусков.
        worksheet.Cells[rowNumber, 1].Value = DirRX.HRManagement.Resources.Overlaps;
        for (var weekCounter = 0; weekCounter < lastWeekNumber; weekCounter++)
        {
          worksheet.Cells[rowNumber, 2 + weekCounter].R1C1Formula = string.Format("=COUNT(R[-{0}]C:R[-1]C)", employeeCounter);
          workbook.CalculateFormula();
          var intersetions = worksheet.Cells[rowNumber, 2 + weekCounter].IntValue;
          // Добавить подсветку, если более 20% отдела уходят в отпуск
          if (intersetions > 1 && 100 * intersetions / (employeeCounter - 1) > peoplePercentage)
            worksheet.Cells[rowNumber, 2 + weekCounter].SetStyle(warningStyle);
        }
        rowNumber++;
        // Сделать пустую общую ячейку в конце каждого подразделения.
        worksheet.Cells.CreateRange(rowNumber, 0, 1, columnCount).Merge();
        rowNumber++;
      }
      
      // Заполнить шапку таблицы.
      var month = 0;
      counter = 1;
      foreach (var beginOfWeek in vacationBeginOfWeeks)
      {
        var firstWeekDate = beginOfWeek.BeginingDate;
        // Объединить ячейки по количеству недель в месяце.
        if (firstWeekDate.Month != month)
        {
          worksheet.Cells[1, 1 + counter].Value = firstWeekDate.ToString("MMMM");
          var weeks = double.Parse((firstWeekDate.EndOfMonth().Subtract(firstWeekDate).Days / 7).ToString());
          var weekCount = int.Parse(Math.Truncate(weeks).ToString()) + 1;
          worksheet.Cells.CreateRange(1, 1 + counter, 1, weekCount).Merge();
          month = firstWeekDate.Month;
        }
        worksheet.Cells[2, 1 + counter].Value = firstWeekDate.ToString("dd.MM.yyyy");
        
        if (Calendar.Between(Calendar.Today, firstWeekDate, firstWeekDate.EndOfWeek()))
          // Отметить текущую дату.
          worksheet.Cells[2, 1 + counter].SetStyle(currentDateStyle);
        else
          worksheet.Cells[2, 1 + counter].SetStyle(dateStyle);

        counter++;
      }
      
      // Добавить легенду.
      rowNumber += 2;
      worksheet.Cells[rowNumber, 3].SetStyle(currentDateStyle);
      worksheet.Cells[rowNumber, 4].SetStyle(leftStyle);
      worksheet.Cells[rowNumber, 4].Value = Resources.VacationChartLegendCurrentDate;
      rowNumber++;
      worksheet.Cells[rowNumber, 3].SetStyle(nextYearStyle);
      worksheet.Cells[rowNumber, 4].SetStyle(leftStyle);
      worksheet.Cells[rowNumber, 4].Value = Resources.VacationChartLegendAnotherYear;
      rowNumber++;
      worksheet.Cells[rowNumber, 3].SetStyle(warningStyle);
      worksheet.Cells[rowNumber, 4].SetStyle(leftStyle);
      worksheet.Cells[rowNumber, 4].Value = Resources.VacationChartLegendWarningFormat(peoplePercentage);
      rowNumber++;
      worksheet.Cells[rowNumber, 3].SetStyle(errorStyle);
      worksheet.Cells[rowNumber, 4].SetStyle(leftStyle);
      worksheet.Cells[rowNumber, 4].Value = Resources.VacationChartLegendError;
      rowNumber++;
      worksheet.Cells[rowNumber, 3].SetStyle(managerStyle);
      worksheet.Cells[rowNumber, 4].SetStyle(leftStyle);
      worksheet.Cells[rowNumber, 4].Value = Resources.VacationChartLegendManager;
    }

    /// <summary>
    /// Получить количество отпускных дней в периоде.
    /// </summary>
    /// <param name="beginPeriod">Дата начала периода.</param>
    /// <param name="endPeriod">Дата конца периода.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Количество дней отпуска.</returns>
    public int GetPeriodAmountVacationDays(DateTime beginPeriod, DateTime endPeriod, IEmployee employee)
    {
      var amountDays = 0;
      var daysInPeriod = endPeriod.Subtract(beginPeriod).Days + 1;
      var checkDate = beginPeriod;
      for (var i = 0; i < daysInPeriod; i++)
      {
        var anyVacation = Vacations.GetAll(l => l.Employee.Equals(employee) && checkDate >= l.StartDate.Value && checkDate <= l.FinDate.Value).Any();
        if (anyVacation)
          amountDays++;
        checkDate = checkDate.NextDay();
      }
      return amountDays;
    }
    
    #endregion Отчет по пересечениям.
    
    #region График отпусков для подписания.
    /// <summary>
    /// Создать график отпусков для подписания.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <param name="year">Год.</param>
    /// <param name="document">Документ.</param>
    /// <returns>График отпусков для подписания.</returns>
    [Remote, Public]
    public virtual IHRDocument CreateNewVacationScheduleDocument(IBusinessUnit businessUnit, DateTime year, IHRDocument document)
    {
      var employee = Employees.As(Users.Current);
      // Получить отчет "График отпусков".
      var report = this.GetVacationScheduleReport(year, businessUnit);
      var internalReport = (Sungero.Reporting.Shared.IInternalReport)report;
      if (internalReport == null)
        return null;
      
      using (var reportStream = new MemoryStream())
      {
        internalReport.InternalExecute(reportStream);
        document.CreateVersionFrom(reportStream, "pdf");
        if (document.State.IsInserted)
        {
          document.LastVersion.Author = Users.Current;
          if (employee != null)
            document.AccessRights.Grant(employee, DefaultAccessRightsTypes.Change);
          document.LastVersion.AssociatedApplication = Sungero.Content.AssociatedApplications.GetAll(app => app.Extension == "pdf").FirstOrDefault();
          document.DocumentKind = DocKindFunctions.GetNativeDocumentKind(DocKind.VacationScheduleKind);
          document.Employee = employee;
          document.BusinessUnit = businessUnit;
          document.Year = year.BeginningOfYear();
        }
        document.Save();
      }
      return document;
    }
    
    /// <summary>
    /// Получить отчет "График отпусков".
    /// </summary>
    /// <param name="year">Год.</param>
    /// <param name="buisnessUnit">Наша организация.</param>
    /// <returns>Отчет.</returns>
    [Public]
    public virtual Sungero.Reporting.IReport GetVacationScheduleReport(DateTime year, IBusinessUnit buisnessUnit)
    {
      var report = Reports.GetVacationScheduleReport();
      report.Year = year;
      report.BuisnessUnit = buisnessUnit;
      return report;
    }
    #endregion

    #region Сводная информация об отпусках.
    
    /// <summary>
    /// Получить информацию по отпускам сотрудника в формате структуры.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Детальная информация по отпускам сотрудника в формате структуры.</returns>
    public virtual Structures.Module.EmployeeVacationData GetVacationDataByEmployee(IEmployee employee)
    {
      var vacations = Functions.Vacation.GetVacations(employee, Calendar.Today);
      
      var vacationData = new Structures.Module.EmployeeVacationData();
      vacationData.VacationDetail = new List<Structures.Module.VacationDataDetail>();
      var vacationKinds = VacationKinds.GetAll().OrderByDescending(l => l.Paid.Value);
      foreach (var vacationKind in vacationKinds)
      {
        var vacationDetail = new Structures.Module.VacationDataDetail();
        var vacationsByKind = vacations.Where(v => v.VacationKind.Equals(vacationKind));
        if (vacationsByKind.Any())
        {
          vacationDetail.VacationKind = vacationKind.Name;
          vacationDetail.Dates = new List<string>();
          foreach (var vacation in vacationsByKind)
            vacationDetail.Dates.Add(string.Format("{0} - {1} ({2}) {3}",
                                                   vacation.StartDate.Value.ToString("dd.MM.yyyy"),
                                                   vacation.FinDate.Value.ToString("dd.MM.yyyy"),
                                                   Functions.Module.GetPeriodAmountPaidDays(vacation.StartDate, vacation.FinDate, vacation.VacationDuration.Value, employee).AmountDaysWithHolidays,
                                                   vacation.Status == HRManagement.Vacation.Status.Closed ? Resources.HeavyCheckMark : string.Empty));
          vacationData.VacationDetail.Add(vacationDetail);
        }
      }

      // Расчёт остатка оплачиваемых дней отпуска.
      var factVacationDaysSum = 0;
      var vacationsWithPay = vacations.Where(v => v.VacationKind.Paid.Value);
      var factVacations = vacationsWithPay.Where(s => s.Status == HRManagement.Vacation.Status.Closed);
      foreach (var factStage in factVacations)
        factVacationDaysSum += factStage.VacationDuration.Value;
      var planSum = 0;
      var planVacations = vacationsWithPay.Where(s => s.Status != HRManagement.Vacation.Status.Shifted);
      foreach (var plan in planVacations)
        planSum += plan.VacationDuration.Value;
      vacationData.PlanedDaysLeft = Resources.PlannedVacationLeftSubjectFormat(planSum - factVacationDaysSum);
      
      // Расчёт использованных дней без сохранения з/п.
      var vacationsWithoutPayCount = 0;
      var vacationsWithoutPay  = vacations.Where(s => s.Status == HRManagement.Vacation.Status.Closed && !s.VacationKind.Paid.Value);
      foreach (var vacation in vacationsWithoutPay)
        vacationsWithoutPayCount += vacation.VacationDuration.Value;
      vacationData.WithoutPayUsed = vacationsWithoutPayCount > 0 ? Resources.WhithoutPayVacationLeftSubjectFormat(vacationsWithoutPayCount) : string.Empty;

      return vacationData;
    }
    
    /// <summary>
    /// Получить информацию по отпускам сотрудника для отображения в "Мои отпуска".
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Детальная информация по отпускам сотрудника.</returns>
    [Public, Remote]
    public virtual string GetVacationFullInfoByEmployee(IEmployee employee)
    {
      var vacationData = this.GetVacationDataByEmployee(employee);
      
      var vacationDetail = string.Empty;
      foreach (var detail in vacationData.VacationDetail)
      {
        if (string.IsNullOrEmpty(vacationDetail))
          vacationDetail += string.Join(Environment.NewLine, string.Format("{0}:", detail.VacationKind));
        else
          vacationDetail += string.Join(Environment.NewLine, string.Format("\n\n{0}:", detail.VacationKind));
        foreach (var dates in detail.Dates)
          vacationDetail += string.Join(Environment.NewLine, string.Format("\n{0}", dates));
      }
      return string.Format("{0}\n\n{1}\n{2}", vacationDetail, vacationData.PlanedDaysLeft, vacationData.WithoutPayUsed);
    }
    
    /// <summary>
    /// Получить информацию по отпускам сотрудника для отображения в "Мои отпуска" в формате json для ЛК.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Детальная информация по отпускам сотрудника в формате json.</returns>
    [Public, Remote]
    public virtual string GetJsonVacationFullInfoByEmployee(IEmployee employee)
    {
      return JsonConvert.SerializeObject(this.GetVacationDataByEmployee(employee));
    }
    
    /// <summary>
    /// Получить кол-во оплачиваемых дней в периоде.
    /// </summary>
    /// <param name="dateBegin">Начало периода.</param>
    /// <param name="dateEnd">Конец периода.</param>
    /// <param name="duration">Продолжительность периода.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <returns>Количество оплачиваемых дней.</returns>
    public DirRX.HRManagement.Structures.Vacation.IAmountDaysInVacation GetPeriodAmountPaidDays(DateTime? dateBegin, DateTime? dateEnd, int duration, IRecipient recipient)
    {
      if (!dateBegin.HasValue || !dateEnd.HasValue)
        return Structures.Vacation.AmountDaysInVacation.Create();
      var holidays = string.Join(", ", this.GetHolidaysInPeriod(dateBegin, dateEnd, recipient).Select(h => h.ToString("dd.MM.yyyy", TenantInfo.Culture)).ToArray());
      var amountDaysWithHolidays = string.Format("{0} дн.{1}", duration, this.GetHolidaysInfoInPeriod(dateBegin, dateEnd, recipient));
      var amountDays = Structures.Vacation.AmountDaysInVacation.Create(duration, holidays, amountDaysWithHolidays);
      
      return amountDays;
    }
    
    /// <summary>
    /// Вернуть даты праздничных дней в отпуске в виде строки.
    /// </summary>
    /// <param name="periodBegin">Дата начала отпуска.</param>
    /// <param name="periodEnd">Дата конца отпуска.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <returns>Даты праздничных дней в виде строки.</returns>
    [Remote]
    public string GetHolidaysInfoInPeriod(DateTime? periodBegin, DateTime? periodEnd, IRecipient recipient)
    {
      if (!periodBegin.HasValue || !periodEnd.HasValue)
        return string.Empty;

      var holidays = this.GetHolidaysInPeriod(periodBegin, periodEnd, recipient);
      if (!holidays.Any())
        return string.Empty;
      
      var firstHoliday = holidays.First();
      var lastHoliday = holidays.OrderByDescending(h => h.Date).First();
      
      if (holidays.Count == 1)
        return string.Format(", есть праздничный день {0}", firstHoliday.ToString("dd.MM", TenantInfo.Culture));
      
      var holidaysInVacation = string.Empty;
      var previousHoliday = firstHoliday;
      
      foreach (var holiday in holidays.Where(h => !h.Equals(firstHoliday)))
      {
        string holidayPeriod;
        if (holiday != previousHoliday.AddDays(1))
        {
          holidayPeriod = firstHoliday == previousHoliday ?
            previousHoliday.ToString("dd.MM", TenantInfo.Culture) : string.Format("{0}-{1}",
                                                                                  firstHoliday.ToString("dd.MM", TenantInfo.Culture),
                                                                                  previousHoliday.ToString("dd.MM", TenantInfo.Culture));
          holidaysInVacation = string.Format("{0}{1}{2}",
                                             holidaysInVacation,
                                             string.IsNullOrEmpty(holidaysInVacation) ? string.Empty : ", ",
                                             holidayPeriod);
          firstHoliday = holiday;
        }
        if (holiday == lastHoliday)
        {
          holidayPeriod = firstHoliday == holiday ?
            holiday.ToString("dd.MM", TenantInfo.Culture) :
            string.Format("{0}-{1}", firstHoliday.ToString("dd.MM", TenantInfo.Culture), holiday.ToString("dd.MM", TenantInfo.Culture));
          holidaysInVacation = string.Format("{0}{1}{2}",
                                             holidaysInVacation,
                                             string.IsNullOrEmpty(holidaysInVacation) ? string.Empty : ", ",
                                             holidayPeriod);
        }
        previousHoliday = holiday;
      }
      return string.Format(", есть праздничные дни {0}", holidaysInVacation);
    }
    
    #endregion
    
    #region Проверки по отпускам.
    
    /// <summary>
    /// Проверить дату окончания отпуска.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="dateBegin">Дата начала отпуска.</param>
    /// <param name="duration">Продолжительность отпуска.</param>
    /// <returns>Сообщение о неправильной дате окончания отпуска.</returns>
    [Remote]
    public string CheckPossibleEndDate(IEmployee employee, DateTime dateBegin, int duration)
    {
      if (duration > 10000)
        return Resources.OverflowDuration;
      var dateEnd = dateBegin.AddDays(duration);
      var yearBorder = Functions.Module.GetBorderVacationYear(employee, dateBegin);
      var borderBeginDate = yearBorder.PreviousYear();
      if (dateEnd > yearBorder)
        return Resources.VacationEndOverBorderCalendarYearFormat(dateBegin.ToShortDateString(),
                                                                 dateEnd.ToShortDateString(),
                                                                 borderBeginDate.ToShortDateString(),
                                                                 yearBorder.ToShortDateString());
      if (!WorkingTime.GetAll(l => l.Year == dateEnd.Year).Any())
        return Resources.NoWorkingTimeCalendarFormat(dateEnd.Year);
      return string.Empty;
    }
    
    /// <summary>
    /// Проверить используется ли план отпусков в организации текущего сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>True - если используется план отпусков.</returns>
    [Remote]
    public bool IsVacationScheduleUsed(IEmployee employee)
    {
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      return Functions.Module.GetModuleSettings(businessUnit).IsScheduleUsed.Value;
    }

    /// <summary>
    /// Получить доступные виды отпусков сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <returns>Список видов отпусков.</returns>
    [Remote]
    public List<IVacationKind> GetAvailableVacationKinds(IEmployee employee, DateTime year)
    {
      var employeeVacationKinds = new List<IVacationKind>();
      var employeeDuration = VacationDurations.GetAll(d => d.Employee.Equals(employee) && d.Year == year).FirstOrDefault();
      if (employeeDuration != null)
      {
        employeeVacationKinds = employeeDuration.AvailableDuration.Where(d => d.DaysCount.Value > 0).Select(k => k.VacationKind).ToList();
      }
      return employeeVacationKinds;
    }
    
    /// <summary>
    /// Проверить корректность указания видов отпусков сотрудника.
    /// </summary>
    /// <param name="vacationDates">Список дат отпусков.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <returns>Ошибки в видах отпусков.</returns>
    [Remote]
    public List<string> CheckVacationKinds(List<Structures.Vacation.IVacationDates> vacationDates, IEmployee employee, DateTime year)
    {
      List<string> errorList = new List<string>();
      var employeeVacationKinds = Functions.Module.GetAvailableVacationKinds(employee, year);
      
      foreach (var vacationData in vacationDates)
      {
        if (!employeeVacationKinds.Contains(vacationData.VacationKind))
        {
          errorList.Add(DirRX.HRManagement.Resources.NotAvailableEmployeeVacationKindFormat(vacationData.VacationKind.Name, vacationData.Duration, year.Year));
        }
      }
      return errorList;
    }
    
    /// <summary>
    /// Вернуть список праздничных дней в периоде.
    /// </summary>
    /// <param name="periodBegin">Начало периода.</param>
    /// <param name="periodEnd">Конец периода.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <returns>Даты праздничных дней.</returns>
    [Public, Remote]
    public List<DateTime> GetHolidaysInPeriod(DateTime? periodBegin, DateTime? periodEnd, IRecipient recipient)
    {
      var holidays = new List<DateTime>();
      if (!periodBegin.HasValue || !periodEnd.HasValue)
        return holidays;

      // Отсортировать даты.
      var dates = new List<DateTime> { periodBegin.Value, periodEnd.Value };
      periodBegin = dates.Min();
      periodEnd = dates.Max();
      
      // Получить праздничные дни из календарей рабочего времени.
      for (var year = periodBegin.Value.Year; year <= periodEnd.Value.Year; year++)
      {
        var workCalendar = Sungero.CoreEntities.WorkingTime.GetAllCachedByYear(year, recipient).Where(n => n.Year == year).FirstOrDefault();
        holidays.AddRange(workCalendar.Day.Where(d => d.Day >= periodBegin && d.Day <= periodEnd
                                                 && d.Kind == Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Holiday).Select(d => d.Day).OrderBy(d => d.Date).ToList());
      }
      return holidays.OrderBy(h => h.Date).Distinct().ToList();
    }

    /// <summary>
    /// Подсчет календарных дней в промежутке времени за вычетом праздничных дней.
    /// </summary>
    /// <param name="startDate">Начало.</param>
    /// <param name="endDate">Окончание.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <returns>Количество календарных дней.</returns>
    [Public, Remote]
    public int DurationInCalendarDaysWihtOutHoliday(DateTime startDate, DateTime endDate, IRecipient recipient)
    {
      var holidaysCount = this.GetHolidaysAmountInPeriod(startDate, endDate, recipient);
      return (int)((TimeSpan)(endDate - startDate)).TotalDays + 1 - holidaysCount;
    }
    
    /// <summary>
    /// Получить количество дней, доступных для компенсации.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="vacKind">Вид отпуска.</param>
    /// <param name="year">Год отпуска.</param>
    /// <param name="availableDays">Количество дней, доступных для планирования.</param>
    /// <returns>Количество дней, доступных для компенсации.</returns>
    [Public, Remote]
    public int GetAvailableDaysToCompensation(IEmployee employee, IVacationKind vacKind, int year, int availableDays)
    {
      int daysToComp = 0;
      // Если использовано дней >= мин кол-ву дней к использованию для вида отпуска, то дней для компенсации = кол-ву дней доступных для планирования.
      // Если использовано дней < мин. кол-ва дней к использованию, то дней для компенсации = кол-ву дней доступных для планирования
      //   за вычетом разницы мин. кол-ва дней для компенсации и кол-ва использованных дней отпуска.
      var vacationUsed = Vacations.GetAll(v => Equals(v.Employee, employee) &&
                                          (v.Year.Value.Year == year) &&
                                          (v.Status == DirRX.HRManagement.Vacation.Status.Closed) &&
                                          Equals(v.VacationKind, vacKind));

      var daysUsed = vacationUsed.Any() ? vacationUsed.Sum(v => v.VacationDuration) : 0;
      
      var minDaysToUse = vacKind.MinDaysToUse != null ? vacKind.MinDaysToUse.Value : 0;
      if (daysUsed > minDaysToUse || (minDaysToUse - daysUsed) > availableDays)
        daysToComp = availableDays;
      else
        daysToComp = (int)(availableDays - (minDaysToUse - daysUsed));
      
      return daysToComp;
    }
    
    /// <summary>
    /// Получить дату окончания отпуска с учетом праздничных дней.
    /// </summary>
    /// <param name="dateBegin">Начало.</param>
    /// <param name="duration">Продолжительность отпуска.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <returns>Дата окончания отпуска с учетом праздничных дней.</returns>
    [Public, Remote]
    public DateTime GetVacationEndDate(DateTime dateBegin, int duration, IRecipient recipient)
    {
      var dateEnd = dateBegin.AddDays(-1);
      var durationWithoutHolidays = 0;
      do
      {
        dateEnd = dateEnd.AddDays(duration - durationWithoutHolidays);
        durationWithoutHolidays = this.DurationInCalendarDaysWihtOutHoliday(dateBegin, dateEnd, recipient);
      }
      while (durationWithoutHolidays != duration);
      return dateEnd;
    }
    
    /// <summary>
    /// Вернуть количество праздничных дней в периоде.
    /// </summary>
    /// <param name="periodBegin">Начало периода.</param>
    /// <param name="periodEnd">Конец периода.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <returns>Количество праздничных дней.</returns>
    public int GetHolidaysAmountInPeriod(DateTime? periodBegin, DateTime? periodEnd, IRecipient recipient)
    {
      if (!periodBegin.HasValue || !periodEnd.HasValue)
        return 0;
      return this.GetHolidaysInPeriod(periodBegin, periodEnd, recipient).Count;
    }
    
    #region Проверки по отпускам в отчете с пересечениями.
    /// <summary>
    /// Проверить правильность указанных дат отпуска в отчете с пересечениями.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <returns>Ошибки в датах отпуска.</returns>
    [Public, Remote]
    public virtual List<string> CheckDateInReport(IEmployee employee, DateTime year)
    {
      var vacationDateList = this.CreateVacationDateStructure(employee, year);
      var errorList = this.CheckVacationsDates(vacationDateList, employee, year);
      
      return errorList;
    }
    
    /// <summary>
    /// Проверить правильность указанных дат отпуска в отчете с пересечениями.
    /// </summary>
    /// <param name="vacationDates">Список дат отпусков.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <returns>Ошибки в датах отпуска.</returns>
    [Public, Remote]
    public virtual List<string> CheckVacationsDates(List<Structures.Vacation.IVacationDates> vacationDates, IEmployee employee, DateTime year)
    {
      List<string> errorList = new List<string>();
      
      // Проверить, что есть часть отпуска с необходимой продолжительностью.
      var vacationDuration = this.CheckVacationRequiredDuration(vacationDates, employee, year);
      if (!string.IsNullOrWhiteSpace(vacationDuration))
        errorList.Add(vacationDuration);
      
      var employeePersonalDuration = Functions.Module.GetEmployeeVacationDuration(employee, year);
      if (employeePersonalDuration != null)
      {
        foreach (var kindDuration in employeePersonalDuration.AvailableDuration)
        {
          var vacationsTotalDuration = Functions.Module.CheckVacationsTotalDuration(vacationDates,
                                                                                    kindDuration.VacationKind,
                                                                                    kindDuration.DaysCount.Value);
          if (!string.IsNullOrWhiteSpace(vacationsTotalDuration))
            errorList.Add(vacationsTotalDuration);
        }
      }
      return errorList;
    }
    #endregion

    #region Создание и подготовка структуры дат отпусков.
    /// <summary>
    /// Подготовить структуру дат из списка отпусков.
    /// </summary>
    /// <param name="vacationList">Список отпусков.</param>
    /// <returns>Список дат отпусков.</returns>
    public virtual List<Structures.Vacation.IVacationDates> FormVacationDateStructure(IQueryable<IVacation> vacationList)
    {
      var vacationDateList = new List<Structures.Vacation.IVacationDates>();
      foreach (var vacation in vacationList)
        vacationDateList.Add(Structures.Vacation.VacationDates.Create(vacation.StartDate.Value,
                                                                      vacation.FinDate.Value,
                                                                      vacation.VacationDuration.Value,
                                                                      vacation.Status == DirRX.HRManagement.Vacation.Status.Closed ? DateStatuses.Closed : DateStatuses.Planned,
                                                                      vacation.VacationKind));
      return vacationDateList;
    }
    
    /// <summary>
    /// Создать структуру для хранения периодов отпусков.
    /// </summary>
    /// <param name="beginDate">Дата начала отпуска.</param>
    /// <param name="endDate">Дата конца отпуска.</param>
    /// <returns>Список дат отпусков.</returns>
    [Public, Remote]
    public virtual Structures.Vacation.IVacationDates CreateVacationDateStructure(DateTime beginDate, DateTime endDate)
    {
      var vacationDates = Structures.Vacation.VacationDates.Create();
      vacationDates.BeginDate = beginDate;
      vacationDates.EndDate = endDate;
      vacationDates.Status = DateStatuses.New;
      return vacationDates;
    }
    
    /// <summary>
    /// Создать структуру для хранения периодов отпусков для проверки дат.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <returns>Список дат отпусков.</returns>
    [Public, Remote]
    public virtual List<Structures.Vacation.IVacationDates> CreateVacationDateStructure(IEmployee employee, DateTime year)
    {
      var vacationList = Functions.Vacation.GetPaidVacations(employee, year);
      return this.FormVacationDateStructure(vacationList);
    }
    
    /// <summary>
    /// Создать структуру для проверки пересечений с учетом отгулов и перенесенных отпусков прошлого и следующего года.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <param name="addNextYear">True - если необходимо добавить даты следующего года.</param>
    /// <returns>Список дат отпусков.</returns>
    public List<Structures.Vacation.IVacationDates> CreateVacationDateStructureForIntersection(IEmployee employee, DateTime year, bool addNextYear)
    {
      var previousYear = year.Year - 1;
      var nextYear = year.Year + 1;
      var vacationList = Vacations.GetAll(v => Equals(v.Employee, employee) &&
                                          (v.Year.Value.Year == previousYear ||
                                           (addNextYear && v.Year.Value.Year == nextYear) ||
                                           (v.Year == year && v.VacationKind.Paid.Value == false)) &&
                                          v.Status != DirRX.HRManagement.Vacation.Status.Closed &&
                                          v.Status != DirRX.HRManagement.Vacation.Status.Canceled &&
                                          v.Status != DirRX.HRManagement.Vacation.Status.Shifted);
      return this.FormVacationDateStructure(vacationList);
    }
    #endregion

    /// <summary>
    /// Проверить, что есть часть основного отпуска с необходимой продолжительностью.
    /// </summary>
    /// <param name="vacationDates">Список дат отпусков.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <param name="year">Год.</param>
    /// <returns>Текст сообщения об отсутствии части основного отпуска с необходимой продолжительностью.</returns>
    [Public]
    public virtual string CheckVacationRequiredDuration(List<Structures.Vacation.IVacationDates> vacationDates, IRecipient recipient, DateTime year)
    {
      // Получить из настроек системы вид основного отпуска и минимальную продолжительность одной из его частей
      var businessUnit = Employees.As(recipient).Department.BusinessUnit;
      var settings = Functions.Module.GetModuleSettings(businessUnit);
      var requiredDuration = settings.RequiredOnePartDuration;
      var mainVacationKind = this.GetPaidVacationKind(recipient);
      
      // Минимальное количество дней основного отпуска требовать только в том случае, если у сотрудника доступно больше минимального необходимого количества дней основного отпуска
      var recipientDuration = VacationDurations.GetAll(d => d.Employee.Equals(recipient) && d.Year == year).FirstOrDefault();
      
      if (recipientDuration != null)
      {
        if (recipientDuration.AvailableDuration.Any(d => d.VacationKind.Equals(mainVacationKind) && requiredDuration <= d.DaysCount.Value))
        {
          if (!vacationDates.Any(d => d.VacationKind.Equals(mainVacationKind) && d.Duration >= requiredDuration))
            return Resources.VacationNoRequiredDurationPartFormat(requiredDuration, mainVacationKind.Name);
        }
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Проверить общую продолжительность отпуска.
    /// </summary>
    /// <param name="vacationDates">Список дат отпусков.</param>
    /// <param name="vacationKind">Вид отпуска.</param>
    /// <param name="limit">Допустимая продолжительность отпуска.</param>
    /// <returns>Текст сообщения о несоответствии продолжительности.</returns>
    [Public]
    public virtual string CheckVacationsTotalDuration(List<Structures.Vacation.IVacationDates> vacationDates,
                                                      IVacationKind vacationKind,
                                                      int limit)
    {
      if (vacationKind.Paid.Value)
      {
        var totalDuration = 0;
        vacationDates = vacationDates.Where(l => Equals(l.VacationKind, vacationKind)).ToList();
        foreach (var vacationPart in vacationDates)
          totalDuration += vacationPart.Duration;
        if (totalDuration < limit)
          return Resources.VacationDurationUnderLimitFormat(vacationKind.Name, limit - totalDuration);
        else if (totalDuration > limit)
          return Resources.VacationDurationOverLimitFormat(vacationKind.Name, totalDuration - limit);
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Проверить, что Дата начала отпуска не больше Даты окончания.
    /// </summary>
    /// <param name="vacationDates">Список дат отпусков.</param>
    /// <returns>Ошибка, если Дата начала отпуска больше Даты окончания.</returns>
    [Public]
    public virtual List<string> CheckVacationDatesRange(List<Structures.Vacation.IVacationDates> vacationDates)
    {
      var error = new List<string>();
      foreach (var vacation in vacationDates)
      {
        if (vacation.BeginDate > vacation.EndDate)
          error.Add(Resources.VacationDatesRangeErrorFormat(vacation.BeginDate.ToShortDateString(), vacation.EndDate.ToShortDateString()));
      }
      return error;
    }
    
    /// <summary>
    /// Проверить, что Дата начала + Длительность = Дата окончания с учетом календаря рабочего времени.
    /// </summary>
    /// <param name="vacationDates">Список дат отпусков.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Ошибка, если Дата начала + Длительность != Дата окончания с учетом календаря рабочего времени.</returns>
    [Public]
    public virtual List<string> CheckVacatinDatesMatchDuration(List<Structures.Vacation.IVacationDates> vacationDates, IEmployee employee)
    {
      var error = new List<string>();
      foreach (var vacation in vacationDates)
      {
        var endDateCalculated = this.GetVacationEndDate(vacation.BeginDate, vacation.Duration, employee);
        if (endDateCalculated != vacation.EndDate)
          error.Add(Resources.VacationDatesNotMatchDurationFormat(vacation.BeginDate.ToShortDateString(), vacation.EndDate.ToShortDateString(), vacation.Duration));
      }
      return error;
    }
    
    /// <summary>
    /// Проверить, что нет отпуска ближе к сегодняшней дате на кол-во дней, указанное в настройках.
    /// </summary>
    /// <param name="vacationDates">Список дат отпусков.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Текст сообщения о недопустимом интервале между сегодняшним днем и датой начала отпуска.</returns>
    [Public]
    public virtual List<string> CheckVacationsInterval(List<Structures.Vacation.IVacationDates> vacationDates, IEmployee employee)
    {
      vacationDates = vacationDates.Where(v => v.Status == DateStatuses.New).ToList();
      var buisnessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var settings = this.GetModuleSettings(buisnessUnit);
      var minInterval = settings.MinDaysForVacationApproval.Value;
      var daysCountPay = settings.DaysCountVacationPay.Value;
      var minimalPossibleDate = this.GetPaymentDayAfterDate(buisnessUnit, Calendar.Today.AddDays(minInterval + 1)).AddDays(daysCountPay + 1);
      var recomendedEndDateWithoutPay = minimalPossibleDate.AddWorkingDays(-1).BeginningOfDay();
      var error = new List<string>();
      foreach (var vacation in vacationDates)
      {
        if (vacation.VacationKind.Paid.Value)
        {
          var endDate = vacation.EndDate;
          var beginDate = vacation.BeginDate;
          if (Calendar.Between(beginDate, Calendar.Today, minimalPossibleDate.AddDays(-1)))
          {
            if (beginDate.Equals(endDate) || beginDate.Equals(recomendedEndDateWithoutPay))
              error.Add(Resources.VacationIntervalIncorrectDayFormat(minimalPossibleDate.ToShortDateString(), beginDate.ToShortDateString()));
            else if (beginDate < recomendedEndDateWithoutPay)
              error.Add(Resources.VacationIntervalIncorrectPeriodFormat(minimalPossibleDate.ToShortDateString(), beginDate.ToShortDateString(),
                                                                        recomendedEndDateWithoutPay > endDate ? endDate.ToShortDateString() : recomendedEndDateWithoutPay.ToShortDateString()));
            else
              error.Add(Resources.VacationIntervalIncorrectFormat(minimalPossibleDate.ToShortDateString()));
          }
        }
      }
      return error;
    }

    /// <summary>
    /// Проверить, что новые даты отпуска не пересекаются.
    /// </summary>
    /// <param name="vacationDates">Список дат отпусков.</param>
    /// <returns>Текст сообщения о том что есть пересечения в отпусках.</returns>
    [Public]
    public virtual List<string> CheckVacationIntersections(List<Structures.Vacation.IVacationDates> vacationDates)
    {
      var error = new List<string>();
      // Исключить мин. даты начала, которые создавались для учета компенсированных  и отозваных дней.
      var vacations = vacationDates.Where(l => l.Status != DateStatuses.Closed && l.BeginDate != DateTime.MinValue).ToList();
      var vacationsCount = vacations.Count();
      
      for (var i = 0; i < vacationsCount - 1; i++)
      {
        for (var j = i + 1; j < vacationsCount; j++)
        {
          var earlyComparedDate = vacations[i].BeginDate > vacations[j].BeginDate ? vacations[j] : vacations[i];
          var lateComparedDate = vacations[i].BeginDate > vacations[j].BeginDate ? vacations[i] : vacations[j];

          if (earlyComparedDate.EndDate == lateComparedDate.BeginDate)
            error.Add(Resources.VacationIntersectionsDayFormat(lateComparedDate.BeginDate.ToShortDateString()));
          else if (earlyComparedDate.EndDate > lateComparedDate.BeginDate)
          {
            var intersectionEndDate = earlyComparedDate.EndDate < lateComparedDate.EndDate ? earlyComparedDate.EndDate : lateComparedDate.EndDate;
            if (lateComparedDate.BeginDate == intersectionEndDate)
              error.Add(Resources.VacationIntersectionsDayFormat(intersectionEndDate.ToShortDateString()));
            else
              error.Add(Resources.VacationIntersectionsPeriodFormat(lateComparedDate.BeginDate.ToShortDateString(), intersectionEndDate.ToShortDateString()));
          }
        }
      }
      
      return error.Distinct().ToList();
    }
    
    /// <summary>
    /// Проверить, что отпуск не заканчивается перед выходным или праздничным днем.
    /// </summary>
    /// <param name="vacationDate">Даты отпуска.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Текст сообщения об окончании отпуска перед выходным или праздничным днем.</returns>
    [Public]
    public virtual string CheckVacationEndBeforeWeekendOrHoliday(Structures.Vacation.IVacationDates vacationDate, IEmployee employee)
    {
      var vacationEnd = vacationDate.EndDate;
      var currentYear = vacationEnd.Year;
      var workCalendar = WorkingTime.GetAllCachedByYear(currentYear, employee).Where(n => n.Year == currentYear).FirstOrDefault();
      var calendarDays = workCalendar.Day;
      // Проверить, что отпуск не заканчивается перед новым годом
      if (calendarDays.Last().Day.Equals(vacationEnd))
        return Resources.VacationEndBeforeWeekendOrHoliday;
      
      var vacationEndDayKind = calendarDays.Where(d => d.Day == vacationEnd.AddDays(1)).First().Kind;
      if (vacationEndDayKind == Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Weekend || vacationEndDayKind == Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Holiday)
        return Resources.VacationEndBeforeWeekendOrHoliday;

      return string.Empty;
    }
    
    /// <summary>
    /// Проверить, что даты отпуска не выходят за границы года отпуска для индивидуального планирования.
    /// </summary>
    /// <param name="vacationDates">Даты отпуска.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год отпуска.</param>
    /// <param name="hintText">Строка с текстом сообщения об ошибке.</param>
    /// <returns>Текст сообщения о выходе дат отпуска за границы периода.</returns>
    [Public]
    public virtual string CheckVacationDateInCalendarYearForSingleSchedule(List<Structures.Vacation.IVacationDates> vacationDates, IEmployee employee, DateTime year, CommonLibrary.LocalizedString hintText)
    {
      var borderEndDate = this.GetBorderVacationYear(employee, year);
      var borderBeginDate = Calendar.Today.AddDays(this.GetModuleSettings(employee.Department.BusinessUnit).DaysCountBeforeVacationAlert.Value);
      return this.CheckVacationDateInCalendarYear(vacationDates, borderBeginDate, borderEndDate, hintText);
    }
    
    /// <summary>
    /// Проверить, что даты отпуска не выходят за границы года отпуска.
    /// </summary>
    /// <param name="vacationDates">Даты отпуска.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год отпуска.</param>
    /// <param name="hintText">Строка с текстом сообщения об ошибке.</param>
    /// <returns>Текст сообщения о выходе дат отпуска за границы периода.</returns>
    [Public]
    public virtual string CheckVacationDateInCalendarYear(List<Structures.Vacation.IVacationDates> vacationDates, IEmployee employee, DateTime year, CommonLibrary.LocalizedString hintText)
    {
      var borderEndDate = this.GetBorderVacationYear(employee, year);
      var borderBeginDate = borderEndDate.PreviousYear();
      return this.CheckVacationDateInCalendarYear(vacationDates, borderBeginDate, borderEndDate, hintText);
    }
    
    /// <summary>
    /// Проверить, что даты отпуска не выходят за границы года отпуска.
    /// </summary>
    /// <param name="vacationDates">Даты отпуска.</param>
    /// <param name="borderBeginDate">Начало учетного периода.</param>
    /// <param name="borderEndDate">Окончание учетного периода.</param>
    /// <param name="hintText">Строка с текстом сообщения об ошибке.</param>
    /// <returns>Текст сообщения о выходе дат отпуска за границы периода.</returns>
    public virtual string CheckVacationDateInCalendarYear(List<Structures.Vacation.IVacationDates> vacationDates, DateTime borderBeginDate, DateTime borderEndDate, CommonLibrary.LocalizedString hintText)
    {
      var error = new List<string>();
      // Исключить мин. даты начала, которые создавались для учета компенсированных  и отозваных дней.
      vacationDates = vacationDates.Where(v => v.Status == DateStatuses.New && v.BeginDate != DateTime.MinValue).ToList();
      foreach (var vacationDate in vacationDates)
      {
        var vacationBegin = vacationDate.BeginDate;
        var vacationEnd = vacationDate.EndDate;
        if (vacationEnd > borderEndDate || vacationBegin < borderBeginDate)
          error.Add(string.Format(hintText, borderBeginDate.ToShortDateString(), borderEndDate.ToShortDateString(), vacationBegin.ToShortDateString(), vacationEnd.ToShortDateString()));
      }
      
      return string.Join(Environment.NewLine, error);
    }
    
    /// <summary>
    /// Получить дату границы года отпуска.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <returns>Дата границы года отпуска.</returns>
    [Public]
    public virtual DateTime GetBorderVacationYear(IEmployee employee, DateTime year)
    {
      return this.GetBorderVacationYear(employee.Department.BusinessUnit, year);
    }
    
    /// <summary>
    /// Получить дату границы года отпуска.
    /// </summary>
    /// <param name="department">Подразделение.</param>
    /// <param name="year">Год.</param>
    /// <returns>Дата границы года отпуска.</returns>
    [Remote, Public]
    public virtual DateTime GetBorderVacationYear(IDepartment department, DateTime year)
    {
      return this.GetBorderVacationYear(department.BusinessUnit, year);
    }
    
    /// <summary>
    /// Получить дату границы года отпуска.
    /// </summary>
    /// <param name="businessUnit">Организация.</param>
    /// <param name="year">Год.</param>
    /// <returns>Дата границы года отпуска.</returns>
    [Remote, Public]
    public virtual DateTime GetBorderVacationYear(IBusinessUnit businessUnit, DateTime year)
    {
      var borderMonth = HRSettingses.Info.Properties.MonthOfVacationYearBorder.GetLocalizedValue(this.GetModuleSettings(businessUnit).MonthOfVacationYearBorder);
      // Дата складывается из месяца указанного в настройках и года, следующего за годом отпуска.
      var borderDateString = string.Format("{0} {1} {2}", Calendar.Today.BeginningOfMonth().Day, borderMonth, year.Year + 1);
      DateTime borderDate = DateTime.ParseExact(borderDateString, "d MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);
      
      return borderDate.EndOfMonth();
    }
    
    #endregion

    #region Работа с задачами.
    
    /// <summary>
    /// Отправка уведомлений о прекращении задачи.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="subject">Тема.</param>
    /// <param name="abortingReason">Причина прекращения.</param>
    /// <param name="author">Инициатор задачи.</param>
    /// <param name="threadSubject">Тема в переписке.</param>
    [Remote, Public]
    public void SendAbortationNotices(Sungero.Workflow.ITask task, string subject, string abortingReason, IUser author, string threadSubject)
    {
      var performers = this.GetAbortationNoticePerformers(task);
      if (performers.Any())
        Sungero.Docflow.PublicFunctions.Module.Remote.SendNoticesAsSubtask(subject, performers, task, abortingReason, author, threadSubject);
    }
    
    /// <summary>
    /// Получить исполнителей для уведомлений об отказе в согласовании.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <returns>Список исполнителей.</returns>
    [Remote]
    public List<IUser> GetAbortationNoticePerformers(Sungero.Workflow.ITask task)
    {
      var performers = Sungero.Workflow.Assignments.GetAll(a => Equals(a.Task, task)).Select(a => a.Performer).ToList();
      return performers;
    }
    
    #endregion

    #region Создание задач.
    
    /// <summary>
    /// Создать и отправить подзадачу на осуществление перевода сотрудника службами.
    /// </summary>
    /// <param name="mainTask">Главная задача.</param>
    public virtual void CreateAndStartTransferServicesSubtask(ITransferTask mainTask)
    {
      var transferServicesTask = TransferServicesTasks.CreateAsSubtask(mainTask);
      transferServicesTask.Employee = mainTask.Employee;
      transferServicesTask.TransferDate = mainTask.TransferDate;
      transferServicesTask.IsTransfer = mainTask.IsTransfer;
      transferServicesTask.EmploymentAttachmentGroup.Employees.Add(mainTask.Employee);
      transferServicesTask.HRDocumentAttachmentGroup.HRDocumentBases.Add(mainTask.AdditionalAgreement);
      transferServicesTask.HRDocumentAttachmentGroup.HROrders.Add(mainTask.Order);
      transferServicesTask.AdditionalAttachmentGroup.All.Concat(mainTask.AdditionalAttachmentGroup.All);
      transferServicesTask.Start();
    }
    
    /// <summary>
    /// Создать задачу на прием.
    /// </summary>
    /// <returns>Задача на прием.</returns>
    [Remote]
    public IHiringTask CreateNewHiringTask()
    {
      return HiringTasks.Create();
    }
    
    /// <summary>
    /// Создать подзадачу на формирование графика отпусков по подразделению.
    /// </summary>
    /// <param name="recipient">Подразделение.</param>
    /// <returns>Созданная подзадача.</returns>
    [Remote(PackResultEntityEagerly = true)]
    public virtual Sungero.Workflow.ITask CreateNewVacationDepartmentSchedulingTask(IRecipient recipient)
    {
      var task = VacationDepartmentSchedulingTasks.Create();
      task.AccessRights.Grant(recipient, DefaultAccessRightsTypes.FullAccess);
      return task;
    }
    
    /// <summary>
    /// Создать задачу на формирование графика отпусков.
    /// </summary>
    /// <returns>Задача на формирование графика отпусков.</returns>
    [Remote]
    public virtual Sungero.Workflow.ITask CreateNewVacationScheduleTask()
    {
      return VacationSchedulingTasks.Create();
    }
    
    /// <summary>
    /// Создать задачу на рассмотрение кандидата.
    /// </summary>
    /// <returns>Задача на рассмотрение кандидата.</returns>
    [Remote]
    public IRecruitmentTask CreateNewRecruitmentTask()
    {
      return RecruitmentTasks.Create();
    }
    
    /// <summary>
    /// Создать подзадачу на согласование документа.
    /// </summary>
    /// <param name="mainTask">Основная задача.</param>
    [Public, Remote]
    public void CreateAndStartDocumentApprovalSubtask(Sungero.Workflow.ITask mainTask)
    {
      var documentApprovalTask = DocumentApprovalTasks.CreateAsSubtask(mainTask);
      documentApprovalTask.Author = mainTask.Author;
      
      var approvers = new List<IRecipient>();
      var performers = new List<IRecipient>();
      var employees = new List<HRSolution.IEmployee>();
      int? approveDeadline = null;
      var isParallel = false;
      var document = InternalDocumentBases.Null;
      var addendaDocuments = new List<IInternalDocumentBase>();
      var others = new List<IEntity>();
      
      if (HRLite.SignTasks.Is(mainTask))
      {
        var task = HRLite.SignTasks.As(mainTask);
        // Заполнить параметры согласования.
        approvers = task.Approvers.Select(s => s.Approver).ToList();
        employees = task.Employees.Select(s => s.Employee).ToList();
        approveDeadline = task.ApproveDeadline;
        isParallel = task.ApprovalQueue == DirRX.HRLite.SignTask.ApprovalQueue.Parallel;
        // Вложить документы.
        document = task.DocumentGroup.BilaterialDocuments.Single();
        addendaDocuments = task.AddendaGroup.BilaterialDocuments.Select(s => InternalDocumentBases.As(s)).ToList();
        others = task.OtherGroup.All.ToList();
      }
      else if (HRLite.HRAcquaintanceTasks.Is(mainTask))
      {
        var task = HRLite.HRAcquaintanceTasks.As(mainTask);
        // Заполнить параметры согласования.
        approvers = task.Approvers.Select(s => s.Approver).ToList();
        performers = task.Performers.Select(s => s.Performer).ToList();
        approveDeadline = task.ApproveDeadline;
        isParallel = task.ApprovalQueue == DirRX.HRLite.HRAcquaintanceTask.ApprovalQueue.Parallel;
        // Вложить документы.
        document = task.DocumentGroup.InternalDocumentBases.Single();
        addendaDocuments = task.AddendaGroup.InternalDocumentBases.ToList();
        others = task.OtherGroup.All.ToList();
      }
      
      foreach (var approver in approvers)
      {
        // Если в качестве согласующего указана роль "Руководители подразделений", то вычислить руководителей указанных сотрудников.
        if (approver.Sid == Sungero.Docflow.Constants.Module.RoleGuid.DepartmentManagersRole)
        {
          // Добавить в согласующие руководителей участников из задачи на ознакомление с КД и ЛНА.
          foreach (var performer in performers)
          {
            var employeesFromRecipient = HRLite.PublicFunctions.Module.Remote.GetEmployeesFromRecipients(new List<IRecipient>{performer});
            
            foreach (var employee in employeesFromRecipient)
            {
              var performerManager = Functions.Module.GetManager(employee);
              
              if (performerManager != null && !documentApprovalTask.Approvers.Select(s => s.Approver.Id).Contains(performerManager.Id))
                documentApprovalTask.Approvers.AddNew().Approver = performerManager;
            }
          }
          
          // Добавить в согласующие руководителей сотрудников из задачи на подписание КДД.
          foreach (var employee in employees)
          {
            var employeeManager = Functions.Module.GetManager(employee);
            
            if (employeeManager != null && !documentApprovalTask.Approvers.Select(s => s.Approver.Id).Contains(employeeManager.Id))
              documentApprovalTask.Approvers.AddNew().Approver = employeeManager;
          }
        }
        // Иначе, добавить согласующего в задачу.
        else
        {
          if (!documentApprovalTask.Approvers.Select(s => s.Approver.Id).Contains(approver.Id))
            documentApprovalTask.Approvers.AddNew().Approver = approver;
        }
      }
      
      documentApprovalTask.ApproveDeadline = approveDeadline;
      documentApprovalTask.IsParallel = isParallel;
      //Вложить документы.
      documentApprovalTask.DocumentGroup.InternalDocumentBases.Add(document);
      foreach (var doc in addendaDocuments)
        documentApprovalTask.AttachmentGroup.InternalDocumentBases.Add(doc);
      foreach (var item in others)
        documentApprovalTask.OtherGroup.All.Add(item);
      
      documentApprovalTask.Start();
    }
    
    #endregion
    
    #region Создание документов.
    
    /// <summary>
    /// Создать кадровый документ.
    /// </summary>
    /// <param name="docKindGuid">Guid вида документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="workDate">Дата выхода сотрудника на работу.</param>
    /// <returns>Документ.</returns>
    [Remote]
    public IHRDocument CreateNewHRDocument(Guid docKindGuid, IEmployee employee, DateTime workDate)
    {
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var document = this.CreateNewHRDocument(docKindGuid, businessUnit);
      document.Employee = employee;
      if (Equals(document.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.TransferAdditionalAgreementKind)) ||
          Equals(document.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.ChangeWorkConditionsAdditionalAgreementKind)))
        document.TransferDate = workDate;
      else
      {
        document.WorkDate = workDate;
        document.Save();
      }
      
      return document;
    }

    /// <summary>
    /// Создать кадровый документ.
    /// </summary>
    /// <param name="docKindGuid">Guid вида документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Документ.</returns>
    [Remote, Public]
    public IHRDocument CreateNewHRDocument(Guid docKindGuid, IEmployee employee)
    {
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var document = this.CreateNewHRDocument(docKindGuid, businessUnit);
      document.Employee = employee;
      document.Save();
      
      return document;
    }
    
    /// <summary>
    /// Создать кадровый документ.
    /// </summary>
    /// <param name="docKindGuid">Guid вида документа.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Документ.</returns>
    [Remote]
    public IHRDocument CreateNewHRDocument(Guid docKindGuid, IBusinessUnit businessUnit)
    {
      var document = HRDocuments.Null;
      var docKind  = DocKindFunctions.GetNativeDocumentKind(docKindGuid);
      // Получить шаблон в зависимости от вида документа.
      var template = GetTemplateByKind(docKind, businessUnit);
      if (template != null)
        document = HRDocuments.CreateFrom(template);
      else
        document = HRDocuments.Create();
      document.DocumentKind = docKind;
      return document;
    }
    
    /// <summary>
    /// Переформировать кадровый документ.
    /// </summary>
    /// <param name="regeneratedDoc">Документ.</param>
    [Remote]
    public void RegenerateDocument(IHRDocumentBase regeneratedDoc)
    {
      // Получить шаблон в зависимости от вида документа.
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(regeneratedDoc.Employee);
      var template = GetTemplateByKind(regeneratedDoc.DocumentKind, businessUnit);
      if (template != null)
      {
        var body = template.LastVersion.Body.Read();
        // Сформировать новую версию из шаблона.
        var newVersion = regeneratedDoc.CreateVersionFrom(body, template.AssociatedApplication.Extension);
        // Заполнить макропеременные.
        var internalEntity = (Sungero.Domain.Shared.IExtendedEntity)regeneratedDoc;
        internalEntity.Params[Sungero.Content.Shared.ElectronicDocumentUtils.FromTemplateIdKey] = template.Id;
        regeneratedDoc.Save();
      }
    }
    
    /// <summary>
    /// Переформировать приказ.
    /// </summary>
    /// <param name="orderDoc">Приказ.</param>
    public void RegenerateOrder(IHROrder orderDoc)
    {
      // Получить шаблон в зависимости от вида документа.
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(orderDoc.Employee);
      var template = GetTemplateByKind(orderDoc.DocumentKind, businessUnit);
      if (template != null)
      {
        var body = template.LastVersion.Body.Read();
        var newVersion = orderDoc.CreateVersionFrom(body, template.AssociatedApplication.Extension);
        var internalEntity = (Sungero.Domain.Shared.IExtendedEntity)orderDoc;
        internalEntity.Params[Sungero.Content.Shared.ElectronicDocumentUtils.FromTemplateIdKey] = template.Id;
        orderDoc.Save();
      }
    }
    
    /// <summary>
    /// Создать приказ.
    /// </summary>
    /// <param name="docKindGuid">Guid вида документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="baseDoc">Документ-основание.</param>
    /// <returns>Приказ.</returns>
    public IHROrder CreateOrder(Guid docKindGuid, IEmployee employee, Sungero.Docflow.IOfficialDocument baseDoc)
    {
      var docKind  = DocKindFunctions.GetNativeDocumentKind(docKindGuid);
      var document = HROrders.Null;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var template = GetTemplateByKind(docKind, businessUnit);
      if (template != null)
        document = HROrders.CreateFrom(template);
      else
        document = HROrders.Create();
      document.DocumentKind = docKind;
      document.Employee = employee;
      document.BaseDocument = baseDoc;
      document.Relations.AddFrom(Constants.Module.Relations.BasisRelationName, baseDoc);
      return document;
    }
    
    /// <summary>
    /// Создать приказ для отпуска.
    /// </summary>
    /// <param name="docKindGuid">Guid вида документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="baseDoc">Документ-основание.</param>
    /// <param name="year">Год отпуска.</param>
    /// <returns>Приказ.</returns>
    public IHROrder CreateOrder(Guid docKindGuid, IEmployee employee, Sungero.Docflow.IOfficialDocument baseDoc, DateTime year)
    {
      var order = this.CreateOrder(docKindGuid, employee, baseDoc);
      order.Year = year;
      return order;
    }
    
    /// <summary>
    /// Создать заявление для отпуска.
    /// </summary>
    /// <param name="docKindGuid">Guid вида документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="date">Дата создания.</param>
    /// <param name="year">Год отпуска.</param>
    /// <returns>Заявление.</returns>
    [Public]
    public IHRDocument CreateStatement(Guid docKindGuid, IEmployee employee, DateTime date, DateTime year)
    {
      var statement = this.CreateStatement(docKindGuid, employee, date);
      statement.Year = year;
      return statement;
    }

    /// <summary>
    /// Создать заявление.
    /// </summary>
    /// <param name="docKindGuid">Guid вида документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="date">Дата создания.</param>
    /// <returns>Заявление.</returns>
    [Public]
    public IHRDocument CreateStatement(Guid docKindGuid, IEmployee employee, DateTime date)
    {
      var docKind = DocKindFunctions.GetNativeDocumentKind(docKindGuid);
      return this.CreateStatement(docKind, employee, date);
    }
    
    /// <summary>
    /// Создать заявление.
    /// </summary>
    /// <param name="docKind">Вид документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="date">Дата создания.</param>
    /// <returns>Заявление.</returns>
    public IHRDocument CreateStatement(IDocumentKind docKind, IEmployee employee, DateTime date)
    {
      var document = HRDocuments.Null;
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var template = GetTemplateByKind(docKind, businessUnit);
      if (template != null)
        document = HRDocuments.CreateFrom(template);
      else
        document = HRDocuments.Create();
      document.DocumentKind = docKind;
      document.Employee = employee;
      document.InitiationDate = date;
      document.LifeCycleState = HRManagement.HRDocument.LifeCycleState.Active;
      return document;
    }

    #endregion
    
    #region Получение документов.
    
    /// <summary>
    /// Получить список заявлений с материальными выплатами.
    /// </summary>
    /// <returns>Список прочих заявлений.</returns>
    [Remote]
    public List<IDocumentKind> GetMaterialStatementDocKind()
    {
      var docKinds = new List<IDocumentKind>();
      docKinds.Add(DocKindFunctions.GetNativeDocumentKind(DocKind.BenefitStatementKind));
      docKinds.Add(DocKindFunctions.GetNativeDocumentKind(DocKind.MaterialAidStatementKind));
      return docKinds;
    }
    
    /// <summary>
    /// Получить кадровые документы сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Документы.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<IHRDocument> GetEmployeeHRDocuments(IEmployee employee)
    {
      var query = HRDocuments.GetAll(d => Equals(d.Employee, employee));
      return query;
    }

    /// <summary>
    /// Получить трудовой договор сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Трудовой договор.</returns>
    [Public, Remote(IsPure = true)]
    public static IHRDocument GetEmployeeContract(IEmployee employee)
    {
      return GetEmployeeHRDocuments(employee).Where(n => Equals(n.DocumentKind, DocKindFunctions.GetNativeDocumentKind(DocKind.EmploymentContractKind))).FirstOrDefault();
    }

    #endregion
    
    #region DocFlow Params.
    
    /// <summary>
    /// Добавление в таблицу параметров Docflow значения.
    /// </summary>
    /// <param name="key">Ключ параметра.</param>
    /// <param name="val">Значение.</param>
    [Remote(IsPure = true)]
    public static void AddDocflowParam(string key, string val)
    {
      Sungero.Docflow.PublicFunctions.Module.InsertOrUpdateDocflowParam(key, val);
    }
    
    /// <summary>
    /// Получить значение параметра Docflow.
    /// </summary>
    /// <param name="key">Ключ параметра.</param>
    /// <returns>Значение.</returns>
    [Public, Remote]
    public string GetDocflowParamValue(string key)
    {
      var command = string.Format(Sungero.Docflow.Queries.Module.SelectDocflowParamsValue, key);
      var commandExecutionResult = Sungero.Docflow.PublicFunctions.Module.ExecuteScalarSQLCommand(command);
      var result = string.Empty;
      if (!(commandExecutionResult is DBNull) && commandExecutionResult != null)
        result = commandExecutionResult.ToString();
      return result;
    }

    #endregion
    
    #region Работа с документами.
    
    /// <summary>
    /// Был ли обновлен документ с момента последнего просмотра.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>True, если пользователь не видел актуальное содержимое документа, иначе false.</returns>
    [Remote(IsPure = true)]
    public static bool DocumentHasBodyUpdateAfterLastView(Sungero.Content.IElectronicDocument document)
    {
      if (!document.HasVersions)
        return false;

      var lastVersionNumber = document.Versions.Max(v => v.Number);
      var lastViewDate = GetDocumentLastViewDate(document, Users.Current);

      // С момента последнего просмотра мной, были ли изменения другими этой версии.
      return lastViewDate == null ||
        document.History.GetAll().Any(
          h => !h.User.Equals(Users.Current) &&
          h.HistoryDate > lastViewDate &&
          h.VersionNumber == lastVersionNumber &&
          ((h.Action == Sungero.CoreEntities.History.Action.Update && h.Operation == Sungero.Content.DocumentHistory.Operation.UpdateVerBody) ||
           (h.Action == Sungero.CoreEntities.History.Action.Update && h.Operation == Sungero.Content.DocumentHistory.Operation.Import) ||
           (h.Action == Sungero.CoreEntities.History.Action.Update && h.Operation == Sungero.Content.DocumentHistory.Operation.CreateVersion) ||
           (h.Action == Sungero.CoreEntities.History.Action.Create && h.Operation == null)));
    }

    /// <summary>
    /// Получить время, когда пользователь последний раз видел тело последней версии документа.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="employee">Пользователь.</param>
    /// <returns>Время.</returns>
    private static DateTime? GetDocumentLastViewDate(Sungero.Content.IElectronicDocument document, IUser employee)
    {
      var lastVersionNumber = document.Versions.Max(v => v.Number);
      return document.History.GetAll()
        .Where(h => h.User.Equals(Users.Current) && h.VersionNumber == lastVersionNumber)
        .Where(h =>
               (h.Action == Sungero.CoreEntities.History.Action.Update && h.Operation == Sungero.Content.DocumentHistory.Operation.UpdateVerBody) ||
               (h.Action == Sungero.CoreEntities.History.Action.Update && h.Operation == Sungero.Content.DocumentHistory.Operation.Import) ||
               (h.Action == Sungero.CoreEntities.History.Action.Update && h.Operation == Sungero.Content.DocumentHistory.Operation.CreateVersion) ||
               (h.Action == Sungero.CoreEntities.History.Action.Create && h.Operation == null) ||
               (h.Action == Sungero.CoreEntities.History.Action.Read && h.Operation == Sungero.Content.DocumentHistory.Operation.ReadVerBody) ||
               (h.Action == Sungero.CoreEntities.History.Action.Read && h.Operation == Sungero.Content.DocumentHistory.Operation.Export))
        .Max(h => h.HistoryDate);
    }

    /// <summary>
    /// Найти список ЛНА для сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Запись справочника Списки ЛНА.</returns>
    [Remote]
    public ILNALists GetLNARecord(IEmployee employee)
    {
      var employeeDepartment = employee.Department;
      var employeeJobTitle = employee.JobTitle;
      var employeeBusinessUnit = employee.Department.BusinessUnit;
      var recordLNA = LNAListses.GetAll(l => l.Departments.Where(k => k.Department.Equals(employeeDepartment)).Any() &&
                                        l.JobTitles.Where(k => k.JobTitle.Equals(employeeJobTitle)).Any() &&
                                        l.BusinessUnit.Equals(employeeBusinessUnit)).FirstOrDefault();
      // Если запись с подразделением и должностью не найдена, то искать только по подразделению.
      if (recordLNA == null)
        recordLNA = LNAListses.GetAll(l => l.Departments.Where(k => k.Department.Equals(employeeDepartment)).Any() &&
                                      !l.JobTitles.Any() &&
                                      l.BusinessUnit.Equals(employeeBusinessUnit)).FirstOrDefault();
      // Если запись с подразделением не найдена, то искать общую запись для НОР.
      if (recordLNA == null)
        recordLNA = LNAListses.GetAll(l => !l.Departments.Any() &&
                                      !l.JobTitles.Any() &&
                                      l.BusinessUnit.Equals(employeeBusinessUnit)).FirstOrDefault();
      return recordLNA;
    }
    
    /// <summary>
    /// Определить, подписываются ли все документы в электронном виде для процессов Движение персонала.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>True, если все документы подписываются в электронном виде.</returns>
    public bool IsAllElectronicStaffDoc(IBusinessUnit businessUnit)
    {
      return this.GetModuleSettings(businessUnit).OriginalDocInStaff == DirRX.HRManagement.HRSettings.OriginalDocInStaff.ElectronicDoc;
    }

    /// <summary>
    /// Определить, подписываются ли только двусторонние документы в бумажном виде для процессов Движение персонала.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>True, если только двусторонние документы подписываются в бумажном виде.</returns>
    public bool IsOnlyBilStaffDocPaper(IBusinessUnit businessUnit)
    {
      return this.GetModuleSettings(businessUnit).OriginalDocInStaff == DirRX.HRManagement.HRSettings.OriginalDocInStaff.OnlyBilDocPaper;
    }

    /// <summary>
    /// Определить, подписываются ли все документы в бумажном виде для процессов Движение персонала.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>True, если все документы подписываются в бумажном виде.</returns>
    public bool IsAllPaperStaffDoc(IBusinessUnit businessUnit)
    {
      return this.GetModuleSettings(businessUnit).OriginalDocInStaff == DirRX.HRManagement.HRSettings.OriginalDocInStaff.AllPaperDoc;
    }
    
    /// <summary>
    /// Определить, подписываются ли все документы Нашей организации в бумажном виде для процессов по отпускам.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>True, если все документы подписываются в бумажном виде, иначе False.</returns>
    public bool IsAllPaperVacationDoc(IBusinessUnit businessUnit)
    {
      // Если включен ЛК в справочнике Настройки подключения к сервисам личного кабинета.
      // Если ЛК не включен - смотрим, подписываются ли документы в бумажном виде, в справочнике Настройки модуля.
      if (EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
      {
        // Смотрим, включен ли ЛК для конкретной Нашей организации.
        var businessUnitO = DirRX.EssPlatformSolution.BusinessUnits.As(businessUnit);
        if (businessUnitO.UseESSDirRX.HasValue && businessUnitO.UseESSDirRX.Value)
          // Если включен - работаем полностью в электронном виде.
          return false;
        else
          // Если нет - смотрим, подписываются ли документы в бумажном виде, в справочнике Настройки модуля.
          return this.GetModuleSettings(businessUnit).OriginalDocInVacation == DirRX.HRManagement.HRSettings.OriginalDocInVacation.AllPaperDoc;
      }
      else
      {
        return this.GetModuleSettings(businessUnit).OriginalDocInVacation == DirRX.HRManagement.HRSettings.OriginalDocInVacation.AllPaperDoc;
      }
    }
    
    /// <summary>
    /// Является ли документ кадровым.
    /// </summary>
    /// <param name="doc">Документ.</param>
    /// <returns>True - является, иначе - нет.</returns>
    [Remote, Public]
    public static bool IsHRDocument(IOfficialDocument doc)
    {
      var documentType = DocumentTypes.GetAll(d => d.DocumentTypeGuid == HRDocument.ClassTypeGuid.ToString() ||
                                              d.DocumentTypeGuid == AspirantDocument.ClassTypeGuid.ToString() ||
                                              d.DocumentTypeGuid == HROrder.ClassTypeGuid.ToString());
      return documentType.Any(n => n.DocumentTypeGuid == doc.DocumentKind.DocumentType.DocumentTypeGuid);
    }
    
    /// <summary>
    /// Получить шаблон документа по виду.
    /// </summary>
    /// <param name="docKind">Вид документа.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Шаблон.</returns>
    [Remote]
    public static Sungero.Content.IElectronicDocumentTemplate GetTemplateByKind(Sungero.Docflow.IDocumentKind docKind, IBusinessUnit businessUnit)
    {
      var setting = Functions.Module.GetModuleSettings(businessUnit);
      var settingTemplate = setting.TemplateSettings
        .Where(l => l.DocKind.Equals(docKind))
        .Select(l => l.Template)
        .First();
      if (settingTemplate != null)
        return Sungero.Content.ElectronicDocumentTemplates.As(settingTemplate);
      else
        return null;
    }
    
    /// <summary>
    /// Создать элемент в очереди агента удаления документов.
    /// </summary>
    /// <param name="document">Документ.</param>
    [Public, Remote]
    public virtual void EnqueueDocumentDeleteJob(Sungero.Docflow.IOfficialDocument document)
    {
      var queueItem = DocumentDeleteQueueItems.Create();
      queueItem.DocumentId = document.Id;
      queueItem.ProcessingStatus = HRManagement.DocumentDeleteQueueItem.ProcessingStatus.NotProcessed;
      queueItem.Save();
    }
    
    /// <summary>
    /// Удалить документ из системы.
    /// </summary>
    /// <param name="document">Документ.</param>
    [Public, Remote]
    public virtual void DocumentDelete(Sungero.Docflow.IOfficialDocument document)
    {
      Sungero.Docflow.OfficialDocuments.Delete(document);
    }
    
    /// <summary>
    /// Получитиь Guid вида документа.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <returns>Guid вида документа.</returns>
    [Public, Remote]
    public static Guid GetDocumentKindGuid(Sungero.Docflow.IDocumentKind documentKind)
    {
      var externalLink = Sungero.Domain.ModuleFunctions.GetAllExternalLinks().Where(l => l.EntityTypeGuid == Sungero.Docflow.Server.DocumentKind.ClassTypeGuid &&
                                                                                    l.EntityId == documentKind.Id).FirstOrDefault();
      if (externalLink == null)
        return Guid.Empty;
      
      return Guid.Parse(externalLink.ExternalEntityId);
    }
    
    #endregion
    
    #region Фильтрации.
    
    /// <summary>
    /// Получить список действующих подразделений по Нашей организации.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Список подразделений.</returns>
    [Remote]
    public List<IDepartment> GetFilteredDepartments(IBusinessUnit businessUnit)
    {
      return this.GetFilteredDepartments(businessUnit, true);
    }
    
    /// <summary>
    /// Получить список подразделений по Нашей организации.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <param name="activeOnly">True если выбрать только действующие подразделения.</param>
    /// <returns>Список подразделений.</returns>
    [Remote]
    public List<IDepartment> GetFilteredDepartments(IBusinessUnit businessUnit, bool activeOnly)
    {
      var departments = Sungero.Company.PublicFunctions.Department.Remote.GetDepartments();
      if (activeOnly)
        departments = departments.Where(d => d.Status == Sungero.CoreEntities.DatabookEntry.Status.Active);
      
      // Подразделения фильтруются по НОР.
      if (businessUnit != null)
        return departments.Where(d => Equals(d.BusinessUnit, businessUnit)).ToList();

      // Подразделения не фильтруются по НОР.
      return departments.ToList();
    }

    /// <summary>
    /// Получить список действующих сотрудников по подразделению.
    /// </summary>
    /// <param name="departments">Список подразделений.</param>
    /// <returns>Список сотрудников.</returns>
    [Remote]
    public List<IEmployee> GetFilteredEmployees(List<IDepartment> departments)
    {
      return this.GetFilteredEmployees(departments, true);
    }
    
    /// <summary>
    /// Получить список сотрудников по подразделению.
    /// </summary>
    /// <param name="departments">Список подразделений.</param>
    /// <param name="activeOnly">True если выбрать только действующих сотрудников.</param>
    /// <returns>Список сотрудников.</returns>
    [Remote]
    public List<IEmployee> GetFilteredEmployees(List<IDepartment> departments, bool activeOnly)
    {
      var employees = Employees.GetAll();
      if (activeOnly)
        employees = employees.Where(l => l.Status == Sungero.Company.Employee.Status.Active);
      
      // Сотрудники фильтруются по подразделению.
      if (departments.Any())
        return employees.Where(d => departments.Contains(d.Department)).ToList();
      
      // Сотрудники не фильтруются по подразделению.
      return employees.ToList();
    }

    /// <summary>
    /// Получить список действующих сотрудников по Нашей организации.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>>
    /// <returns>Список сотрудников.</returns>
    [Remote]
    public List<IEmployee> GetFilteredEmployees(IBusinessUnit businessUnit)
    {
      return this.GetFilteredEmployees(businessUnit, true);
    }
    
    /// <summary>
    /// Получить список сотрудников по Нашей организации.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>>
    /// <param name="activeOnly">True если выбрать только действующих сотрудников.</param>
    /// <returns>Список сотрудников.</returns>
    [Remote]
    public List<IEmployee> GetFilteredEmployees(IBusinessUnit businessUnit, bool activeOnly)
    {
      var employees = Employees.GetAll();
      if (activeOnly)
        employees = employees.Where(l => l.Status == Sungero.Company.Employee.Status.Active);
      
      // Сотрудники фильтруются по НОР.
      if (businessUnit != null)
        return employees.Where(d => Equals(d.Department.BusinessUnit, businessUnit)).ToList();
      
      // Сотрудники не фильтруются по НОР.
      return employees.ToList();
    }
    
    #endregion
    
    #region Разное.
    
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <param name="text">Текст инструкции.</param>
    /// <returns>Модель состояния.</returns>
    public StateView GetAssignmentStateView(string text)
    {
      var stateView = Sungero.Core.StateView.Create();
      var block = stateView.AddBlock();
      var content = block.AddContent();
      content.AddLabel(text);
      block.ShowBorder = false;
      return stateView;
    }

    /// <summary>
    /// Сериализовать объект в Json.
    /// </summary>
    /// <param name="obj">Объект.</param>
    /// <returns>Строка в формате JSON.</returns>
    [Public]
    public string SerializedToJson(object obj)
    {
      return JsonConvert.SerializeObject(obj);
    }
    
    /// <summary>
    /// Получить подчиненные НОР.
    /// </summary>
    /// <param name="headBusinessUnits">Головные НОР.</param>
    /// <returns>Подчиненные НОР.</returns>
    public List<Sungero.Company.IBusinessUnit> GetSubBusinessUnits(List<Sungero.Company.IBusinessUnit> headBusinessUnits)
    {
      var subBusinessUnits = BusinessUnits.GetAll(d => headBusinessUnits.Contains(d.HeadCompany)).ToList();
      
      if (subBusinessUnits.Any())
        subBusinessUnits.AddRange(this.GetSubBusinessUnits(subBusinessUnits));
      
      return subBusinessUnits.Distinct().ToList();
    }
    
    /// <summary>
    /// Получить подчиненные подразделения.
    /// </summary>
    /// <param name="headDepartments">Головные подразделения.</param>
    /// <returns>Подчиненные подразделения.</returns>
    public List<Sungero.Company.IDepartment> GetSubDepartments(List<Sungero.Company.IDepartment> headDepartments)
    {
      var departments = Departments.GetAll().ToList();
      var subDepartments = departments.Where(d => headDepartments.Any(hd => hd.Equals(d.HeadOffice))).ToList();
      if (subDepartments.Any())
        subDepartments.AddRange(this.GetSubDepartments(subDepartments));
      
      return subDepartments.Distinct().ToList();
    }
    
    #endregion
    
    #region Замещения.
    
    /// <summary>
    /// Создать замещение.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <param name="substitute">Замещающий пользователь.</param>
    /// <returns>Замещение.</returns>
    [Remote]
    public static ISubstitution CreateSubstitution(IUser user, IUser substitute)
    {
      if (!Substitutions.GetAll().Any(x => user.Equals(x.User) && substitute.Equals(x.Substitute)))
      {
        var substitution = Substitutions.Create();
        substitution.User = user;
        substitution.Substitute = substitute;
        substitution.IsSystem = true;
        substitution.Comment = string.Empty;
        substitution.Save();
        return substitution;
      }
      return null;
    }
    
    /// <summary>
    /// Удалить замещение.
    /// </summary>
    /// <param name="substitution">Замещение.</param>
    [Remote]
    public void DeleteSubstitution(ISubstitution substitution)
    {
      // Удалить замещение.
      if (Substitutions.GetAll().Any(x => substitution.Equals(x)))
      {
        Substitutions.Delete(substitution);
      }
    }
    
    /// <summary>
    /// Отправить уведомления о заданиях тем, кто замещает одну персону (совместителям).
    /// </summary>
    [Public]
    [Remote]
    public virtual void SendNotificationToWhoSubstitute()
    {
      var notificationDate = Calendar.Now;
      var previousRun = HRManagement.Server.ModuleFunctions.GetLastNotificationDate();
      var newAssignments = this.GetNewAssignments(previousRun, notificationDate);
      var expiredAssignments = this.GetExpiredAssignments(previousRun, notificationDate);
      var allAssignments = new List<IAssignmentBase>();
      allAssignments.AddRange(newAssignments);
      allAssignments.AddRange(expiredAssignments);
      allAssignments = allAssignments.Distinct().ToList();
      
      
      var allWhoSubstitute = Substitutions.GetAll(s => s.IsSystem != true)
        .Where(s => Employees.As(s.Substitute).Person.Id == Employees.As(s.User).Person.Id)
        .Select(s => s.Substitute).Distinct().ToList();

      foreach (var whoSubstitute in allWhoSubstitute)
      {
        var employeeWhoSubstitute = Employees.As(whoSubstitute);
        
        var assignentsOfPerson = allAssignments.Where(a => Employees.As(a.Performer).Person.Id == employeeWhoSubstitute.Person.Id).ToList();
        var assignentsToEmployee = allAssignments.Where(a => Employees.As(a.Performer).Id == employeeWhoSubstitute.Id).ToList();
        var assignentsOfPersonWithoutEmployee = assignentsOfPerson.Except(assignentsToEmployee).ToList();
        
        
        if (assignentsOfPersonWithoutEmployee.Count > 0)
        {
          var empHyperlinks = new StringBuilder();
          foreach (var assignment in assignentsOfPersonWithoutEmployee)
          {
            empHyperlinks.Append(Hyperlinks.Get(assignment));
            empHyperlinks.Append(Environment.NewLine);
          }
          
          var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(DirRX.HRManagement.Resources.AssignmentsToWhoSubstitute, employeeWhoSubstitute);
          task.ActiveText = empHyperlinks.ToString().Trim();
          task.Start();
        }
      }
      
      using (new Sungero.Domain.Session())
      {
        if (allAssignments.Any())
          UpdateLastNotificationDate(notificationDate);
        else
          Logger.Debug("HRManagement.ModuleServerFunctions.SendNotificationsToWhoSubstitute. Last notification date hasn't been changed");
      }
    }
    
    /// <summary>
    /// Обновить дату последней рассылки уведомлений совместителям.
    /// </summary>
    /// <param name="notificationDate">Дата рассылки уведомлений.</param>
    public static void UpdateLastNotificationDate(DateTime notificationDate)
    {
      var key = "LastNotificationOfAssignmentWhoSubstitute";
      
      var newDate = notificationDate.Add(-Calendar.UtcOffset).ToString("yyyy-MM-ddTHH:mm:ss.ffff+0");
      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Sungero.Docflow.Queries.Module.InsertOrUpdateDocflowParamsValue, new[] { key, newDate });
      Logger.DebugFormat("HRManagement.ModuleServerFunctions.UpdateLastNotificationDate. Last notification date to employees who substitute is set to {0} (UTC)", newDate);
    }
    
    /// <summary>
    /// Получить дату последней рассылки уведомлений совместителям.
    /// </summary>
    /// <returns>Дата последней рассылки.</returns>
    public static DateTime GetLastNotificationDate()
    {
      var key = "LastNotificationOfAssignmentWhoSubstitute";
      var command = string.Format(Sungero.Docflow.Queries.Module.SelectDocflowParamsValue, key);
      try
      {
        var executionResult = Sungero.Docflow.PublicFunctions.Module.ExecuteScalarSQLCommand(command);
        var date = string.Empty;
        if (!(executionResult is DBNull) && executionResult != null)
          date = executionResult.ToString();
        Logger.DebugFormat("HRManagement.ModuleServerFunctions.GetLastNotificationDate. Last notification date in DB is {0} (UTC)", date);
        
        DateTime result = Calendar.FromUtcTime(DateTime.Parse(date, null, System.Globalization.DateTimeStyles.AdjustToUniversal));

        if ((result - Calendar.Now).TotalDays > 1)
          return Calendar.Today;
        else
          return result;
      }
      catch (Exception ex)
      {
        Logger.Error("HRManagement.ModuleServerFunctions.GetLastNotificationDate. Error while getting last notification date", ex);
        return Calendar.Today;
      }
    }
    
    /// <summary>
    /// Получить новые задания, по которым надо сделать рассылку совместителям.
    /// </summary>
    /// <param name="previousRun">Предыдущий запуск.</param>
    /// <param name="notificationDate">Текущий запуск.</param>
    /// <returns>Задания, по которым будет выполнена рассылка.</returns>
    public virtual List<IAssignmentBase> GetNewAssignments(DateTime previousRun, DateTime notificationDate)
    {
      return AssignmentBases
        .GetAll(a => previousRun <= a.Created && a.Created < notificationDate && a.IsRead == false && a.Status != Sungero.Workflow.AssignmentBase.Status.Aborted)
        .Expand("Performer")
        .ToList();
    }
    
    /// <summary>
    /// Получить просроченные задания, по которым надо сделать рассылку совместителям.
    /// </summary>
    /// <param name="previousRun">Предыдущий запуск.</param>
    /// <param name="notificationDate">Текущий запуск.</param>
    /// <returns>Задания, по которым будет выполнена рассылка.</returns>
    public virtual List<IAssignment> GetExpiredAssignments(DateTime previousRun, DateTime notificationDate)
    {
      return Assignments
        .GetAll(a => a.Status == Sungero.Workflow.AssignmentBase.Status.InProcess  &&
                (a.Deadline.HasValue && a.Deadline.Value.HasTime() &&
                 previousRun <= a.Deadline && a.Deadline < notificationDate ||
                 a.Deadline.HasValue && !a.Deadline.Value.HasTime() &&
                 previousRun <= a.Deadline.Value.AddDays(1) && a.Deadline.Value.AddDays(1) < notificationDate))
        .Expand("Performer")
        .ToList();
    }
    
    #endregion

    #region Функции для работы с личным кабинетом через сервис интеграции.
    
    /// <summary>
    /// Получить BlockUid.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <param name="type">Тип задания (assignment/notice).</param>
    /// <returns>BlockUid.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetBlockUid(int assignmentId, string type)
    {
      if (type.ToLower() == "assignment")
        return Sungero.Workflow.Assignments.Get(assignmentId).BlockUid;
      if (type.ToLower() == "notice")
        return Sungero.Workflow.Notices.Get(assignmentId).BlockUid;
      return string.Empty;
    }
    
    /// <summary>
    /// Получить ИД вида отпуска с помощью ИД отпуска.
    /// </summary>
    /// <param name="vacationId">ИД отпуска</param>
    /// <returns>ИД вида отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public int? GetVacationKindIdFromVacationId(int vacationId)
    {
      var vacation = Vacations.Get(vacationId);
      return vacation?.VacationKind.Id;
    }
    
    /// <summary>
    /// Заполнить планируемые отпуска.
    /// </summary>
    /// <param name="assignmentId">ИД задания на планирование отпуска.</param>
    /// <param name="vacationKindIds">ИД видов отпусков.</param>
    /// <param name="vacationBeginDates">Даты начала отпусков.</param>
    /// <param name="VacationEndDates">Даты окончания отпусков.</param>
    /// <param name="vacationDurations">Продолжительности отпусков.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void FillVacations(int assignmentId, List<int?> vacationKindIds, List<DateTime?> vacationBeginDates, List<DateTime?> vacationEndDates, List<int?> vacationDurations)
    {
      var assignment = HRManagement.VacationEmployeeSchedulingAssignments.Get(assignmentId);
      if (assignment != null)
      {
        assignment.Vacations.Clear();

        for (var i = 0; i <= vacationKindIds.Count - 1; i++)
        {
          var vacation = assignment.Vacations.AddNew();
          vacation.DateBegin = vacationBeginDates[i];
          vacation.DateEnd = vacationEndDates[i];
          vacation.Duration = vacationDurations[i];
          vacation.VacationKind = HRManagement.VacationKinds.GetAll(x => x.Id == vacationKindIds[i]).FirstOrDefault();
          vacation.Number = i+1;
        }
        assignment.IsFromEssAccount = true;
        assignment.Save();
      }
    }
    
    /// <summary>
    /// Проверить на заполненность поля "Вид отпуска".
    /// </summary>
    /// <param name="vacationBeginDates">ИД видов отпусков.</param>
    /// <returns>True, если заполнены, false - если нет.</returns>
    public bool IsVacationIdFilled(List<int?> vacationKindIds)
    {
      if (vacationKindIds.Any() && !vacationKindIds.Contains(null))
        return true;
      return false;
    }
    
    /// <summary>
    /// Проверить на заполненность поля "Начало отпуска".
    /// </summary>
    /// <param name="vacationBeginDates">Даты начала отпусков.</param>
    /// <returns>True, если заполнены, false - если нет.</returns>
    public bool IsBeginDateFilled(List<DateTime?> vacationBeginDates)
    {
      if (vacationBeginDates.Any() && !vacationBeginDates.Contains(null))
        return true;
      return false;
    }

    /// <summary>
    /// Проверить на заполненность поля "Конец отпуска".
    /// </summary>
    /// <param name="VacationEndDates">Даты окончания отпусков.</param>
    /// <returns>True, если заполнены, false - если нет.</returns>
    public bool IsEndDateFilled(List<DateTime?> vacationEndDates)
    {
      if (vacationEndDates.Any() && !vacationEndDates.Contains(null))
        return true;
      return false;
    }

    /// <summary>
    /// Проверить на заполненность поля "Количество дней отпуска".
    /// </summary>
    /// <param name="vacationDurations">Продолжительности отпусков.</param>
    /// <returns>True, если заполнены, false - если нет.</returns>
    public bool IsDurationFilled(List<int?> vacationDurations)
    {
      if (vacationDurations.Any())
      {
        foreach (var duration in vacationDurations)
        {
          if (duration == null)
            return false;
        }
        return true;
      }
      return false;
    }
    
    /// <summary>
    /// Получение инструкции для доработки параметров переноса отпуска.
    /// </summary>
    /// <returns>Инструкция.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetInstructionReworkVacation()
    {
      return DirRX.HRManagement.Resources.ReworkVacationInstruction;
    }
    
    /// <summary>
    /// Кнопка для виджета в ЛК.
    /// </summary>
    /// <returns>Структура отображения кнопки.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string VacationDayOffButton()
    {
      return "{\"buttons\": [ { \"action\": {\"type\": \"RUN_FACILITY\", \"parameters\": {\"id\": \"d1899307-092b-4a08-87b5-3c7c14f2feff\"} },\"caption\": \"Оформить отгул\", \"type\": \"plain-button\", \"align\": \"center\", \"outlined\": \"true\" }]}";
    }

    /// <summary>
    /// Кнопка для виджета Перенос отпуска в ЛК.
    /// </summary>
    /// <returns>Структура отображения кнопки Перенос отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string ShiftVacationButton()
    {
      return "{\"buttons\": [ { \"action\": {\"type\": \"RUN_FACILITY\", \"parameters\": {\"id\": \"35e8174a-68f9-470f-9eb5-fb92673aee62\"} }, \"caption\": \"Перенос отпуска\", \"type\": \"plain-button\", \"align\": \"center\", \"outlined\": \"true\" }]}";
    }
    
    /// <summary>
    /// Кнопка для виджета Перенос отпуска в ЛК.
    /// </summary>
    /// <returns>Структура отображения кнопки Перенос отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string ShiftVacationButton2()
    {
      return "{\"buttons\": [ { \"action\": {\"type\": \"RUN_FACILITY\", \"parameters\": {\"id\": \"6e7a8c95-aa2b-44f3-ae0c-6f748a0e68ac\"} }, \"caption\": \"Перенос отпуска\", \"type\": \"plain-button\", \"align\": \"center\", \"outlined\": \"true\" }]}";
    }
    
    /// <summary>
    /// Кнопки для виджета Сервисы отпуска в ЛК.
    /// </summary>
    /// <returns>Структура отображения кнопок Перенос отпуска и Оформление отгула.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string VacationButtons()
    {
      return "{\"buttons\": [{ \"action\": {\"type\": \"RUN_FACILITY\", \"parameters\": {\"id\": \"d1899307-092b-4a08-87b5-3c7c14f2feff\"} },\"caption\": \"Оформить отгул\", \"type\": \"plain-button\", \"align\": \"center\", \"outlined\": \"true\" }, { \"action\": {\"type\": \"RUN_FACILITY\", \"parameters\": {\"id\": \"6e7a8c95-aa2b-44f3-ae0c-6f748a0e68ac\"} }, \"caption\": \"Перенос отпуска\", \"type\": \"plain-button\", \"align\": \"center\", \"outlined\": \"true\" }]}";
    }
    
    /// <summary>
    /// Получение инструкции для планирования отпуска.
    /// </summary>
    /// <param name="assignmentId">Задание на планирование оптуска.</param>
    /// <returns>Инструкция.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual List<string> GetTableInstruction(int assignmentId)
    {
      var vacationEmployeeSchedulingAssignments = DirRX.HRManagement.VacationEmployeeSchedulingAssignments.GetAll(assignment => assignment.Id == assignmentId).FirstOrDefault();
      var result = Functions.Module.TrimInstruction(VacationDepartmentSchedulingTasks.Resources.FillInstructionFormat(DirRX.HRManagement.VacationDepartmentSchedulingTasks.As(vacationEmployeeSchedulingAssignments.Task).Year.Value.Year));
      return result.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
    }
    
    /// <summary>
    /// Получение последнего дня отпуска с учетом рабочего календаря.
    /// </summary>
    /// <param name="dateBegin">Начало отпуска.</param>
    /// <param name="duration">Продолжительность отпуска.</param>
    /// <param name="userId">ИД сотрудника.</param>
    /// <returns>Последний день отпуска с учетом рабочего календаря.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public DateTime GetVacationEndDate(DateTime dateBegin, int duration, int userId)
    {
      var employee = Employees.GetAll(x => x.Id == userId).FirstOrDefault();
      var recipient = Recipients.As(employee);
      return this.GetVacationEndDate(dateBegin, duration, recipient);
    }
    
    /// <summary>
    /// Получить доступные для распределения дни отпуска.
    /// </summary>
    /// <param name="userId">ИД сотрудника.</param>
    /// <returns>Доступные для распределения дни отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetAvailableVacationDays(int userId)
    {
      var result = new List<Structures.Module.IAvailableVacationDays>();
      var employee = Employees.GetAll(x => x.Id == userId).FirstOrDefault();
      
      // HACK особенности сервиса ЛК
      var assignment = DirRX.HRManagement.VacationEmployeeSchedulingAssignments.GetAll(x => x.Performer == Users.As(employee)
                                                                                       && x.Status == DirRX.HRManagement.VacationEmployeeSchedulingAssignment.Status.InProcess).FirstOrDefault();
      var year = VacationDepartmentSchedulingTasks.As(assignment.Task).Year.Value;
      
      var vacationDurations = this.GetEmployeeVacationDuration(employee, year);
      if (vacationDurations != null)
      {
        foreach (var duration in vacationDurations.AvailableDuration)
        {
          var availableVacationDays = Structures.Module.AvailableVacationDays.Create();
          availableVacationDays.Name = duration.VacationKind.Name;
          availableVacationDays.Count = duration.DaysCount.Value;
          availableVacationDays.Id = duration.VacationKind.Id;
          result.Add(availableVacationDays);
        }
      }
      
      return JsonConvert.SerializeObject(result);
    }
    
    /// <summary>
    /// Получение даных об отгуле, который надо доработать, через сервис интеграции.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Сериализованная строка с информацией об отпуске.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetCurrentDayOff(int assignmentId)
    {
      var assignment = VacationStatementApprovalAssignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      var result = Structures.Module.CurrentDayOff.Create();
      
      result.VacationId = assignment.VacationKind.Id;
      result.Begin = assignment.DateBegin.Value;
      result.Duration = assignment.Duration.Value;
      result.End = assignment.DateEnd.Value;
      
      return JsonConvert.SerializeObject(result);
    }
    
    /// <summary>
    /// Получение даных об отказанном отгуле через сервис интеграции.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Сериализованная строка с информацией об отпуске.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetDeniedDayOff(int assignmentId)
    {
      var task = VacationApprovalTasks.As(VacationNotices.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task);
      var result = Structures.Module.CurrentDayOffStringDate.Create();
      result.VacationId = task.VacationKind.Id;
      result.Begin = task.DateBegin.Value.ToString("yyyy-MM-dd");
      result.Duration = task.Duration.Value;
      result.End = task.DateEnd.Value.ToString("yyyy-MM-dd");
      return JsonConvert.SerializeObject(result);
    }
    
    /// <summary>
    /// Получение даных об оптуске на перенос, который надо доработать, через сервис интеграции.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Сериализованная строка с информацией об отпуске.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetCurrentShiftVacation(int assignmentId)
    {
      var assignment = ShiftStatementApprovalAssignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      var result = Structures.Module.CurrentDayOff.Create();
      result.VacationId = assignment.Vacation1.Id;
      
      var vacation = Vacations.GetAll(x => x.Id == assignment.Vacation1.Id).FirstOrDefault();
      
      result.Begin = vacation.StartDate.Value;
      result.Duration = vacation.VacationDuration.Value;
      result.End = vacation.FinDate.Value;
      
      return JsonConvert.SerializeObject(result);
    }
    
    /// <summary>
    /// Получение даных об отказанном отпуске на перенос, который надо доработать, через сервис интеграции.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Сериализованная строка с информацией об отпуске.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetDeniedShiftVacation(int assignmentId)
    {
      var task = VacationShiftTasks.As(ShiftNotices.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task);
      var result = Structures.Module.CurrentDayOffStringDate.Create();
      result.VacationId = task.Vacation1.Id;
      
      var vacation = Vacations.GetAll(x => x.Id == task.Vacation1.Id).FirstOrDefault();
      
      result.Begin = vacation.StartDate.Value.ToString("yyyy-MM-dd");
      result.Duration = vacation.VacationDuration.Value;
      result.End = vacation.FinDate.Value.ToString("yyyy-MM-dd");
      
      return JsonConvert.SerializeObject(result);
    }
    
    /// <summary>
    /// Получить причину отказа переноса отпуска.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Причина отказа.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetDenyShiftReason(int assignmentId)
    {
      var task = ShiftNotices.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task;
      var assignments = ShiftStatementApprovalAssignments.GetAll(x => Equals(x.Task, task) && x.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId && x.Id != assignmentId);
      
      if (assignments.Any())
        return assignments.OrderByDescending(x => x.Created).FirstOrDefault().ActiveText;
      
      return string.Empty;
    }
    
    /// <summary>
    /// Получить причину отказа в оформлении отгула.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Причина отказа.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetDenyDayOffReason(int assignmentId)
    {
      var task = VacationNotices.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task;
      var assignments = VacationStatementApprovalAssignments.GetAll(x => Equals(x.Task, task) && x.BlockUid == Constants.VacationApprovalTask.ApprovingBlockId && x.Id != assignmentId);
      
      if (assignments.Any())
        return assignments.OrderByDescending(x => x.Created).FirstOrDefault().ActiveText;
      
      return string.Empty;
    }
    
    /// <summary>
    /// Получить причину отправки на доработку задания на оформление отгула.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Причина отправки на доработку.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetReworkShiftReason(int assignmentId)
    {
      var task = Sungero.Workflow.Assignments.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task;
      var assignments = ShiftStatementApprovalAssignments.GetAll(x => Equals(x.Task, task) && x.BlockUid == Constants.VacationShiftTask.ManagerApprovingAssignmentBlockId.ToString() && x.Id != assignmentId);
      
      if (assignments.Any())
        return assignments.OrderByDescending(x => x.Created).FirstOrDefault().ActiveText;
      
      return string.Empty;
    }
    
    /// <summary>
    /// Получить причину отправки на доработку задания на оформление переноса отпуска.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Причина отправки на доработку.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetReworkDayOffReason(int assignmentId)
    {
      var task = Sungero.Workflow.Assignments.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task;
      var assignments = VacationStatementApprovalAssignments.GetAll(x => Equals(x.Task, task) && x.BlockUid == Constants.VacationApprovalTask.ApprovingBlockId.ToString() && x.Id != assignmentId);
      
      if (assignments.Any())
        return assignments.OrderByDescending(x => x.Created).FirstOrDefault().ActiveText;
      
      return string.Empty;
    }
    
    /// <summary>
    /// Сборка данных для виджета "Отправленные заявления".
    /// </summary>
    /// <param name="userId">Ид сотрудника.</param>
    /// <returns>Данные для виджета "Отправленные заявления".</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationTaskDataForWidget(int userId)
    {
      var taskDataList = new List<Structures.Module.TaskDataForWidget>();
      var tasks = Sungero.Workflow.Tasks.GetAll(t => t.Status != Sungero.Workflow.Task.Status.Draft && t.Author.Id == userId &&
                                                (VacationApprovalTasks.Is(t) || (VacationShiftTasks.Is(t) && !(VacationShiftTasks.As(t).IsVacationRecall.HasValue && VacationShiftTasks.As(t).IsVacationRecall.Value))));
      foreach (var task in tasks)
      {
        var taskData = Structures.Module.TaskDataForWidget.Create();
        taskData.Id = task.Id;
        taskData.ProcessName = VacationApprovalTasks.Is(task) ? "IVacationApprovalTasks" : "IVacationShiftTasks";
        taskData.Subject = VacationApprovalTasks.Is(task) ?
          Resources.DayOffSubjectInWidgetFormat(VacationApprovalTasks.As(task).DateBegin.Value.ToString("dd.MM.yyyy"), VacationApprovalTasks.As(task).DateEnd.Value.ToString("dd.MM.yyyy")) :
          Resources.ShiftVacationSubjectInWidgetFormat(VacationShiftTasks.As(task).DataBegin1.Value.ToString("dd.MM.yyyy"), VacationShiftTasks.As(task).DataEnd1.Value.ToString("dd.MM.yyyy"));
        taskData.Created = task.Created.Value;
        taskData.Status = task.Status.Value.ToString();
        taskDataList.Add(taskData);
      }
      return JsonConvert.SerializeObject(taskDataList);
    }
    
    /// <summary>
    /// Получить признак склееных отпусков в задании на подтверждение отпуска.
    /// </summary>
    /// <param name="assignmentId">Ид задания на подтверждение отпуска.</param>
    /// <returns>Признак склееных отпусков в задании на подтверждение отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public bool IsApprovalVacationGlued(int assignmentId)
    {
      var vacationsCount = VacationAlertTasks.As(VacationApprovalAssignments.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task).Vacations.Count;
      return vacationsCount > 1 ? true : false;
    }
    
    /// <summary>
    /// Получение даных об оптуске на перенос, который стартован из задания на подтверждение.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Сериализованная строка с информацией об отпуске.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetApprovalVacationOnRework(int assignmentId)
    {
      var vacation = VacationAlertTasks.As(VacationApprovalAssignments.GetAll(x => x.Id == assignmentId).FirstOrDefault().Task).Vacation;
      var result = Structures.Module.CurrentDayOff.Create();
      
      result.VacationId = vacation.Id;
      result.Begin = vacation.StartDate.Value;
      result.Duration = vacation.VacationDuration.Value;
      result.End = vacation.FinDate.Value;
      
      return JsonConvert.SerializeObject(result);
    }
    
    /// <summary>
    /// Получить инструкцию для карточки подтверждения отпуска без склеенных отпусков.
    /// </summary>
    /// <returns>Инструкция.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacAppAInstruction()
    {
      return Resources.VacAppIstructionEss.ToString().Replace(Environment.NewLine, @"<br/>");
    }
    
    /// <summary>
    /// Получить инструкцию для карточки подтверждения отпуска со склеенными отпусками.
    /// </summary>
    /// <returns>Инструкция.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacAppAGluedInstruction()
    {
      return Resources.VacAppIstructionGluedEss.ToString().Replace(Environment.NewLine, @"<br/>");
    }
    
    /// <summary>
    /// Получить инструкцию для карточки отказа от подписания.
    /// </summary>
    /// <returns>Инструкция для карточки отказа от подписания.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetRefusalInstruction()
    {
      return Resources.RefusalInstruction.ToString();
    }
    
    /// <summary>
    /// Получить инструкцию для услуги отправки на доработку документов на согласование.
    /// </summary>
    /// <returns>Инструкция для услуги отправки на доработку документов на согласование.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetDocumentApprovalOnRewokInstruction()
    {
      return Resources.DocumentApprovalOnReworkInstruction.ToString();
    }
    
    /// <summary>
    /// Получить информацию по отпускам сотрудника для отображения в "Мои отпуска" в формате json для ЛК.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Детальная информация по отпускам сотрудника в формате json.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string GetJsonVacationFullInfoByEmployee(int employeeId)
    {
      var employee = Sungero.Company.Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      return DirRX.HRManagement.PublicFunctions.Module.Remote.GetJsonVacationFullInfoByEmployee(employee);
    }
    
    /// <summary>
    /// Сборка JSON для виджета "Заявления сотрудника".
    /// </summary>
    /// <param name="userId">Ид сотрудника.</param>
    /// <returns>Cтрока с json для услуг виджета "Заявления сотрудника".</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string GetJSONForStatementsWidget(int userId)
    {
      string jsonForStatementsWidget = "[]";
      var employee = Sungero.Company.Employees.Get(userId);
      var allEmployees = HRSolution.Employees.GetAll(e => e.Status == Sungero.Company.Employee.Status.Active && Equals(e.Person, employee.Person)).ToList();
      if (allEmployees.Where(e => HRLite.PublicFunctions.Module.Remote.CheckEmployeeConsent(e)).Any())
        jsonForStatementsWidget = DirRX.HRLite.PublicFunctions.Module.Remote.GetJSONForStatementsWidget(userId);
      return jsonForStatementsWidget;
      
    }
    
    /// <summary>
    /// Получить ФИО отказавшего в рассмотрении/исполнении заявления для картоки в ЛК.
    /// </summary>
    /// <param name="assignmentId">ИД уведомления.</param>
    /// <returns>ФИО отказавшего.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetStatementDenyPerson(int assignmentId)
    {
      return DirRX.HRLite.PublicFunctions.Module.Remote.GetStatementDenyPerson(assignmentId);
    }
    
    /// <summary>
    /// Получить причину отказа в рассмотрении/исполнении заявления для картоки в ЛК.
    /// </summary>
    /// <param name="assignmentId">ИД уведомления.</param>
    /// <returns>Причина отказа.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetStatementDenyReason(int assignmentId)
    {
      return DirRX.HRLite.PublicFunctions.Module.Remote.GetStatementDenyReason(assignmentId);
    }
    
    /// <summary>
    /// Получить текст задачи на информирование сотрудников.
    /// </summary>
    /// <param name="assignmentId">ИД уведолмения.</param>
    /// <returns>Текст задачи.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetInformingNoticeActiveText(int assignmentId)
    {
      return DirRX.HRLite.PublicFunctions.Module.Remote.GetInformingNoticeActiveText(assignmentId);
    }
    
    /// <summary>
    /// Получить причину отказа подписания кадровых договорных документов.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Причина отказа.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetDenySignReason(int assignmentId)
    {
      return DirRX.HRLite.PublicFunctions.Module.Remote.GetDenySignReason(assignmentId);
    }
    
    /// <summary>
    /// Получить отказавшего подписания кадровых договорных документов.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Причина отказа.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetDenySignPerson(int assignmentId)
    {
      return DirRX.HRLite.PublicFunctions.Module.Remote.GetDenySignPerson(assignmentId);
    }
    
    /// <summary>
    /// Получить текст задачи на подписание кадровых договорных документов.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Текст задачи.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetSignTaskActiveText(int assignmentId)
    {
      return DirRX.HRLite.PublicFunctions.Module.Remote.GetSignTaskActiveText(assignmentId);
    }
    
    /// <summary>
    /// Получить текст задачи на подписание кадровых документов.
    /// </summary>
    /// <param name="assignmentId">Ид задания.</param>
    /// <returns>Текст задачи.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetAcquaintanceTaskActiveText(int assignmentId)
    {
      return DirRX.HRLite.PublicFunctions.Module.Remote.GetAcquaintanceTaskActiveText(assignmentId);
    }

    #endregion
  }
}