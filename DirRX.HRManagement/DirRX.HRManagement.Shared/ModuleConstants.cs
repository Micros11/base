using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class Module
  {

    /// <summary>
    /// Максимальное количество итераций АО для обновления свойства "Согласие на КЭДО".
    /// </summary>
    public const int EmployeesConsentUpdateMaxIteration = 5;

    // Типы связей документов
    // TODO класс определен на случай увеличения кол-ва типов связей, оставить одну константу, если останется только эта связь
    public static class Relations
    {
      /// <summary>
      /// Имя связи "Основание".
      /// </summary>
      public const string BasisRelationName = "Basis";
    }
    
    /// <summary>
    /// Перевод Мб в байты и наоборот 1024*1024.
    /// </summary>
    public const int ConvertMb = 1048576;
    
    /// <summary>
    /// Максимальное количество выгружаемых документов.
    /// </summary>
    public const int ExportedDocumentsCountMaxLimit = 1000;
    
    /// <summary>
    /// Максимальное количество файлов в выгружаемом zip архиве.
    /// </summary>
    public const int ExportedFilesCountMaxLimit = 5000;
    
    /// <summary>
    /// Максимальная сумма размеров файлов в выгружаемом zip архиве.
    /// </summary>
    public const int ExportedFilesSizeMaxLimitMb = 450;

    /// <summary>
    /// Количество страниц документа, на которых ищутся якоря, для добавления отметки об ЭП.
    /// </summary>
    public const int SearchablePagesLimit = 100;

    public static class HRRoles
    {
      /// <summary>
      /// Guid роли Все пользователи.
      /// </summary>
      [Public]
      public static readonly Guid AllUsers = Guid.Parse("440103EA-A766-47A8-98AD-5260CA32DE46");
      
      /// <summary>
      /// Guid роли Пользователи решения HRPro.
      /// </summary>
      [Public]
      public static readonly Guid HRProUsers = Guid.Parse("B98D07AA-D9D6-42A4-BA22-855B8918A7D0");
      
      /// <summary>
      /// Guid роли Пользователи модуля Движение персонала.
      /// </summary>
      [Public]
      public static readonly Guid PersonnelFlowUsers = Guid.Parse("9F05A697-45F9-4395-BD5C-CF47828F67D0");
      
      /// <summary>
      /// Guid роли Пользователи модуля Пользователи модуля Отпуска.
      /// </summary>
      [Public]
      public static readonly Guid VacationsUsers = Guid.Parse("0FD036D7-0959-42AB-9E32-342E7B4F2117");
      
      /// <summary>
      /// Guid роли Специалист ИТ-подразделения.
      /// </summary>
      [Public]
      public static readonly Guid ITResponsible = Guid.Parse("141c7c6f-7dee-41e2-95df-e52e8a71bb84");
      
      /// <summary>
      /// Guid роли Специалист административно-хозяйственной службы.
      /// </summary>
      [Public]
      public static readonly Guid SupplyResponsible = Guid.Parse("e2eb093c-c21f-4e5a-9e19-d273379a5dc7");
      
      /// <summary>
      /// Guid роли Специалист службы безопасности.
      /// </summary>
      [Public]
      public static readonly Guid SecurityResponsible = Guid.Parse("b33b3b69-e9a6-4826-999a-e40129cbfd7e");
      
      /// <summary>
      /// Guid роли Директор по персоналу.
      /// </summary>
      [Public]
      public static readonly Guid StaffChief = Guid.Parse("1affd20c-f352-485a-81c8-9b4845ab2756");
      
      /// <summary>
      /// Guid роли Специалист бухгалтерии.
      /// </summary>
      [Public]
      public static readonly Guid Accounter = Guid.Parse("776a0864-3df4-4c7d-83cf-851c0b3f7065");

      /// <summary>
      /// Guid роли Ответственный за график отпусков в организации.
      /// </summary>
      [Public]
      public static readonly Guid VacationResponsible = Guid.Parse("8b84aa9c-85ce-448c-8816-2a687c307d48");
      
      /// <summary>
      /// Guid роли Ответственные за график отпусков в подразделении.
      /// </summary>
      [Public]
      public static readonly Guid VacationManagers = Guid.Parse("ce8b0749-b6f8-4ebf-9b3f-525bf84921f1");
      
      /// <summary>
      /// Guid роли Ответственные за настройку модуля HR-процессы.
      /// </summary>
      [Public]
      public static readonly Guid HRSettingsManager = Guid.Parse("5f78ca21-c717-4b5e-9329-721664125b80");
      
      /// <summary>
      /// Guid роли Администратор HR-процессов.
      /// </summary>
      [Public]
      public static readonly Guid HRAdministrator = Guid.Parse("5c5de8e6-d00a-48d6-9d15-725928a25572");
      
      /// <summary>
      /// Guid роли Ответственный за выгрузку кадровых документов.
      /// </summary>
      [Public]
      public static readonly Guid HRDocExportManager = Guid.Parse("9353351b-c7b4-47bc-82f0-84e04a08b432");
      
    }
    
    /// <summary>
    /// Guid роли Пользователи HR-процессов, для которых включен режим отладки.
    /// </summary>
    [Public]
    public static readonly Guid HRDebugUsers = Guid.Parse("5d401c0c-7184-411a-afa8-cbaf1ed70782");
    
    /// <summary>
    /// Guid видов документов.
    /// </summary>
    public static class DocumentKind
    {
      #region Прием.
      /// <summary>
      /// Guid вида документа Соглашение об электронном взаимодействии.
      /// </summary>
      public static readonly Guid ElectronicInteractionAgreementKind = Guid.Parse("a40cb988-7b38-4346-b4f9-aac9e9b335fb");
      
      /// <summary>
      /// Guid вида документа Заявление о присоединении к электронному взаимодействию.
      /// </summary>
      public static readonly Guid JoiningStatementKind = Guid.Parse("9511dfa5-f57d-4840-92ef-da837e779bc5");
      
      /// <summary>
      /// Guid вида документа Трудовой договор.
      /// </summary>
      public static readonly Guid EmploymentContractKind = Guid.Parse("b5fcee14-81f8-4c38-9bcb-bcfecdaaa34e");
      
      /// <summary>
      /// Guid вида документа Согласие на обработку персональных данных.
      /// </summary>
      public static readonly Guid ProcPersonalDataAgreementKind = Guid.Parse("db0b0b5f-382a-4520-8bbd-53b2b9282982");

      /// <summary>
      /// Guid вида документа Приказ о приеме.
      /// </summary>
      public static readonly Guid HiringOrderKind = Guid.Parse("3d47e95c-8627-4f1e-8b61-7886c55b01b9");
      
      /// <summary>
      /// Guid вида документа Лист ознакомления со списком ЛНА.
      /// </summary>
      public static readonly Guid AcquaintanceListKind = Guid.Parse("67798bc2-5b25-4cfb-a12f-cd772b0432c0");
      #endregion
      
      #region Перевод.
      /// <summary>
      /// Guid вида документа Доп. соглашение при переводе в другое подразделение.
      /// </summary>
      public static readonly Guid TransferAdditionalAgreementKind = Guid.Parse("decb14fc-a8b0-495c-8fda-13e9d01a6545");
      
      /// <summary>
      /// Guid вида документа Доп. соглашение при изменении условий труда.
      /// </summary>
      public static readonly Guid ChangeWorkConditionsAdditionalAgreementKind = Guid.Parse("54c8d5f4-e991-4ca4-8466-b5157ce0de9e");
      
      /// <summary>
      /// Guid вида документа Заявление на перевод.
      /// </summary>
      public static readonly Guid TransferStatementKind = Guid.Parse("1cf4cf55-0c0f-4234-ae66-a4a4223d1046");
      
      /// <summary>
      /// Guid вида документа Приказ о переводе.
      /// </summary>
      public static readonly Guid TransferOrderKind = Guid.Parse("43964259-5bdb-45b6-9edc-2a875eb3fa86");
      
      /// <summary>
      /// Guid вида документа Приказ об изменении условий труда.
      /// </summary>
      public static readonly Guid ChangeWorkConditionsOrderKind = Guid.Parse("2a802120-d4d3-4d60-a819-057f76bf2130");
      
      #endregion
      
      #region Увольнение.
      /// <summary>
      /// Guid вида документа Заявление на увольнение.
      /// </summary>
      public static readonly Guid DismissalStatementKind = Guid.Parse("4414a1f5-a2be-4504-a9ac-c688dd177834");
      
      /// <summary>
      /// Guid вида документа Приказ об увольнении.
      /// </summary>
      public static readonly Guid DismissalOrderKind = Guid.Parse("41e4d758-57be-4826-b357-0a253ec7057b");
      #endregion
      
      #region Отпуск.
      /// <summary>
      /// Guid вида документа Заявление на отпуск.
      /// </summary>
      public static readonly Guid VacationStatementKind = Guid.Parse("e8943430-d118-4391-a3b3-0fe7d5e32d13");
      
      /// <summary>
      /// Guid вида документа Заявление на перенос отпуска.
      /// </summary>
      public static readonly Guid VacationShiftStatementKind = Guid.Parse("6aa4c449-7a3f-4e31-9e06-18571ca16463");
      
      /// <summary>
      /// Guid вида документа Заявление на отзыв из отпуска.
      /// </summary>
      public static readonly Guid VacationRecallStatementKind = Guid.Parse("6a07a4bb-73a4-4dc0-83f9-0c81fdb09e0b");
      
      /// <summary>
      /// Guid вида документа Приказ об отпуске.
      /// </summary>
      [Public]
      public static readonly Guid VacationOrderKind = Guid.Parse("df0867e9-3743-4d20-b99e-460098091b1b");
      
      /// <summary>
      /// Guid вида документа Приказ о переносе отпуска.
      /// </summary>
      public static readonly Guid VacationShiftOrderKind = Guid.Parse("2239d58d-f9d9-4525-be19-ba212ecc744b");
      
      /// <summary>
      /// Guid вида документа Приказ об отзыве из отпуска.
      /// </summary>
      public static readonly Guid VacationRecallOrderKind = Guid.Parse("ca86f6fa-a801-46e1-bdf6-3d0d1b06fd80");
      
      /// <summary>
      /// Guid вида документа Уведомление о начале отпуска.
      /// </summary>
      [Public]
      public static readonly Guid ScheduledVacationNoticeKind = Guid.Parse("b9ed8ff3-3bf5-4064-833b-8f07494df0b6");
      
      /// <summary>
      /// Guid вида документа Лист ознакомления с графиком отпусков.
      /// </summary>
      public static readonly Guid VacationScheduleAcquaintanceListKind = Guid.Parse("d58296a9-dfa7-405f-b94b-81bc43d6048f");
      #endregion
      
      #region Подбор.
      /// <summary>
      /// Guid вида документа Отчет по кандидату.
      /// </summary>
      public static readonly Guid AspirantReportKindName = Guid.Parse("e9ac4658-6a9e-46af-8483-b67e68ef0b0b");
      
      /// <summary>
      /// Guid вида документа Прочие документы по кандидату.
      /// </summary>
      public static readonly Guid OtherAspirantDoctKindName = Guid.Parse("2801ffca-c3cd-4599-9def-a2ead936274e");
      #endregion
      
      #region Прочие.
      /// <summary>
      /// Guid вида документа Заявление на выплату пособия.
      /// </summary>
      public static readonly Guid BenefitStatementKind = Guid.Parse("fb0b79f0-5281-4c02-95c7-dbe03bb5b1f0");
      
      /// <summary>
      /// Guid вида документа Заявление на выплату материальной помощи.
      /// </summary>
      public static readonly Guid MaterialAidStatementKind = Guid.Parse("3f34af2e-34ca-4f0e-94e7-909ab03b3559");
      
      /// <summary>
      /// Guid вида документа Заявление на изменение персональных данных.
      /// </summary>
      public static readonly Guid ChangePersonalDataStatementKind = Guid.Parse("a5d3fd6e-5b35-44d3-a0bb-138732eb47ae");
      
      /// <summary>
      /// Guid вида документа График отпусков.
      /// </summary>
      [Public]
      public static readonly Guid VacationScheduleKind = Guid.Parse("325b0c52-Cbb7-494e-8af5-0f509bd0f73d");
      #endregion
      
    }
    
    /// <summary>
    /// Актуальная версия типов задач HR-процессов.
    /// </summary>
    public static class HRTasksActualVersion
    {
      /// <summary>
      /// Увольнение сотрудника.
      /// </summary>
      [Public]
      public const string Dismissal = "V2";
      
      /// <summary>
      /// Прием сотрудника.
      /// </summary>
      [Public]
      public const string Hiring = "";
      
      /// <summary>
      /// Рассмотрение кандидата.
      /// </summary>
      [Public]
      public const string Recruitment = "";
      
      /// <summary>
      /// Изменение условий труда сотрудника.
      /// </summary>
      [Public]
      public const string Transfer = "";
      
      /// <summary>
      /// Подтверждение дат отпуска.
      /// </summary>
      [Public]
      public const string VacationAlert = "";
      
      /// <summary>
      /// Согласование отпуска.
      /// </summary>
      [Public]
      public const string VacationApproval = "";
      
      /// <summary>
      /// Планирование отпусков в подразделении.
      /// </summary>
      [Public]
      public const string VacationDepartmentScheduling = "";
      
      /// <summary>
      /// Планирование отпусков.
      /// </summary>
      [Public]
      public const string VacationScheduling = "";
      
      /// <summary>
      /// Изменение отпуска.
      /// </summary>
      [Public]
      public const string VacationShift = "";
    }
    
    /// <summary>
    /// Ключи параметров Docflow.
    /// </summary>
    public static class ParamKey
    {
      /// <summary>
      /// Шаблон ключа для параметра режима планирования отпусков по НОР.
      /// </summary>
      [Public]
      public const string SchedulingKeyBusinessUnit = "DirRX.HRSolution.SchedulingBusinessUnit.{0}";
      
      /// <summary>
      /// Шаблон ключа для параметра режима планирования отпусков по подразделению.
      /// </summary>
      [Public]
      public const string SchedulingKeyDepartment = "DirRX.HRSolution.SchedulingDepartment.{0}";
      
      /// <summary>
      /// Ключ периода мониторинга задач на планирование в подразделениях, в секундах.
      /// </summary>
      [Public]
      public const string SchedulingMonitoringTimeKey = "DirRX.HRSolution.SchedulingMonitoringTime";
      
      /// <summary>
      /// Ключ периода мониторинга в задаче на подтверждение отпуска, в секундах.
      /// </summary>
      [Public]
      public const string AlertMonitoringTimeKey = "DirRX.HRSolution.AlertMonitoringTime";
      
      /// <summary>
      /// Пользователь для работы с личным кабинетом.
      /// </summary>
      [Public]
      public const string PersonalAccountUser = "DirRX.HRSolution.PersonalAccountUser";
    }
    
    /// <summary>
    /// Статусы дат отпусков.
    /// </summary>
    public static class DateStatus
    {
      /// <summary>
      /// Статус дат исполненного (фактически отгулянного) отпуска.
      /// </summary>
      [Public]
      public const string Closed = "Исполнен";

      /// <summary>
      /// Статус дат ранее созданного, еще не закрытого отпуска: на утверждении, запланирован, подтвержден или исполняется.
      /// </summary>
      [Public]
      public const string Planned = "Запланирован";
      
      /// <summary>
      /// Статус дат планируемого прямо сейчас отпуска.
      /// </summary>
      [Public]
      public const string New = "Новый";
    }
    
    /// <summary>
    /// Guid типов задач.
    /// </summary>
    public static class TaskTypeGuid
    {
      /// <summary>
      /// Guid задачи на увольнение.
      /// </summary>
      public static readonly Guid DismissalType = Guid.Parse("aa53b02d-af0e-42ad-bef8-83c41dd6ff85");
      
      /// <summary>
      /// Guid задачи на приём.
      /// </summary>
      public static readonly Guid HiringType = Guid.Parse("ec1b64f6-82e4-46e1-a5a5-dd3dd6be0456");
      
      /// <summary>
      /// Guid задачи на рассмотрение кандидата.
      /// </summary>
      public static readonly Guid RecruitmentType = Guid.Parse("ac6b2553-7d49-468e-a28b-e005fb65ee03");
      
      /// <summary>
      /// Guid задачи на изменение условий труда сотрудника службами компании.
      /// </summary>
      public static readonly Guid TransferServicesType = Guid.Parse("30058f88-58d4-4105-a2b2-803b463404c4");
      
      /// <summary>
      /// Guid задачи на изменение условий труда.
      /// </summary>
      public static readonly Guid TransferType = Guid.Parse("01940621-365a-4767-a824-1e240b2db781");
      
      /// <summary>
      /// Guid задачи на подтверждение дат отпуска.
      /// </summary>
      public static readonly Guid VacationAlertType = Guid.Parse("49f18d7c-bd12-411b-b16e-ce44228d8bf2");
      
      /// <summary>
      /// Guid задачи на согласование заявления на отпуск.
      /// </summary>
      public static readonly Guid VacationApprovalType = Guid.Parse("4b6e5d34-800b-41d2-a56d-02c48a04d420");
      
      /// <summary>
      /// Guid задачи на согласование графика отпусков в подразделениях.
      /// </summary>
      public static readonly Guid VacationDepartmentSchedulingType = Guid.Parse("6103c687-e605-4579-beaf-ab9c0af62299");
      
      /// <summary>
      /// Guid задачи на планирование графика отпусков.
      /// </summary>
      public static readonly Guid VacationSchedulingType = Guid.Parse("c4d246fe-bd71-4cdf-a488-1395d72a824c");
      
      /// <summary>
      /// Guid задачи на изменение отпуска.
      /// </summary>
      public static readonly Guid VacationShiftType = Guid.Parse("cdbe3f36-59cd-45c9-958d-131b2958b2f4");
      
      /// <summary>
      /// Guid задачи на планирование отпуска сотрудника.
      /// </summary>
      public static readonly Guid VacationSingleSchedulingType = Guid.Parse("6afc460e-8582-457d-a277-05e1a533fa03");
    }
  }
}