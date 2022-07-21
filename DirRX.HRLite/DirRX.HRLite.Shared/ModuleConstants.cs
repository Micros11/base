using System;
using Sungero.Core;

namespace DirRX.HRLite.Constants
{
  public static class Module
  {

    // Операции с документами.
    public static class Operation
    {
      // Изменение тела документа.
      public const string UpdateVerBody = "UpdateVerBody";
    }

    /// <summary>
    /// Настройка заявлений.
    /// </summary>
    public static class StatementsSetting
    {
      #region Шаблоны для видов документов.      
      // Шаблоны для вида документа Прочее заявление.
      public static class OtherStatementRXKind
      {
        // Имя шаблона ЛК.
        public const string TemplateName = "Шаблон прочего заявления (КЭДО-Заявления)";
        
        // Имя шаблона RX.
        public const string RXTemplateName = "Шаблон прочего заявления (КЭДО-Заявление RX)";
      }
      
      // Шаблоны для вида документа Заявление на изменение персональных данных.
      public static class ChangePersonalDataStatementKind
      {
        // Имя шаблона ЛК.
        public const string TemplateName = "Шаблон заявления на изменение персональных данных (КЭДО-Заявления)";
        
        // Имя шаблона RX.
        public const string RXTemplateName = "Шаблон заявления на изменение персональных данных (КЭДО-Заявление RX)";
      }
      
      // Шаблоны для вида документа Заявление о переводе.
      public static class TransferStatementKind
      {
        // Имя шаблона ЛК.
        public const string TemplateName = "Шаблон заявления о переводе (КЭДО-Заявления)";
        
        // Имя шаблона RX.
        public const string RXTemplateName = "Шаблон заявления о переводе (КЭДО-Заявление RX)";
      }
      
      // Шаблоны для вида документа Заявление о переносе отпуска.
      public static class VacationChangePeriodStatementKind
      {
        // Имя шаблона ЛК.
        public const string TemplateName = "Шаблон заявления о переносе отпуска (КЭДО-Заявления)";
        
        // Имя шаблона RX.
        public const string RXTemplateName = "Шаблон заявления о переносе отпуска (КЭДО-Заявление RX)";
      }
      
      // Шаблоны для вида документа Заявление о предоставлении дополнительного дня отдыха в связи со сдачей крови.
      public static class DonorAdditionalRestDayStatementKind
      {
        // Имя шаблона ЛК.
        public const string TemplateName = "Шаблон заявления о предоставлении дополнительного дня отдыха в связи со сдачей крови (КЭДО-Заявления)";
        
        // Имя шаблона RX.
        public const string RXTemplateName = "Шаблон заявления о предоставлении дополнительного дня отдыха в связи со сдачей крови (КЭДО-Заявление RX)";
      }
      
      // Шаблоны для вида документа Заявление о предоставлении ежегодного оплачиваемого отпуска.
      public static class AnnualPaidVacationStatementKind
      {
        // Имя шаблона ЛК.
        public const string TemplateName = "Шаблон заявления о предоставлении ежегодного оплачиваемого отпуска (КЭДО-Заявления)";
        
        // Имя шаблона RX.
        public const string RXTemplateName = "Шаблон заявления о предоставлении ежегодного оплачиваемого отпуска (КЭДО-Заявление RX)";
      }
      
      // Шаблоны для вида документа Заявление о предоставлении отпуска без сохранения заработной платы.
      public static class VacationWithoutPayStatementKind
      {
        // Имя шаблона ЛК.
        public const string TemplateName = "Шаблон заявления о предоставлении отпуска без сохранения заработной платы (КЭДО-Заявления)";
        
        // Имя шаблона RX.
        public const string RXTemplateName = "Шаблон заявления о предоставлении отпуска без сохранения заработной платы (КЭДО-Заявление RX)";
      }
      
      // Шаблоны для вида документа Заявление о предоставлении отпуска по беременности и родам.
      public static class MaternityLeaveStatementKind
      {
        // Имя шаблона ЛК.
        public const string TemplateName = "Шаблон заявления о предоставлении отпуска по беременности и родам (КЭДО-Заявления)";
        
        // Имя шаблона RX.
        public const string RXTemplateName = "Шаблон заявления о предоставлении отпуска по беременности и родам (КЭДО-Заявление RX)";
      }
      
      // Шаблоны для вида документа Заявление о предоставлении учебного отпуска.
      public static class StudyLeaveStatementKind
      {
        // Имя шаблона ЛК.
        public const string TemplateName = "Шаблон заявления о предоставлении учебного отпуска (КЭДО-Заявления)";
        
        // Имя шаблона RX.
        public const string RXTemplateName = "Шаблон заявления о предоставлении учебного отпуска (КЭДО-Заявление RX)";
      }
      
      // Шаблоны для вида документа Заявление об оказании материальной помощи.
      public static class FinancialAssistanceStatementKind
      {
        // Имя шаблона ЛК.
        public const string TemplateName = "Шаблон заявления об оказании материальной помощи (КЭДО-Заявления)";
        
        // Имя шаблона RX.
        public const string RXTemplateName = "Шаблон заявления об оказании материальной помощи (КЭДО-Заявление RX)";
      }
      
      // Шаблоны для вида документа Заявление об увольнении.
      public static class DismissalStatementKind
      {
        // Имя шаблона ЛК.
        public const string TemplateName = "Шаблон заявления об увольнении (КЭДО-Заявления)";
        
        // Имя шаблона RX.
        public const string RXTemplateName = "Шаблон заявления об увольнении (КЭДО-Заявление RX)";
      }
      
      #endregion
    }
    
    /// <summary>
    /// Guid видов документов.
    /// </summary>
    public static class DocumentKind
    {
      #region Кадровые договорные документы.
      /// <summary>
      /// Guid вида документа Трудовой договор.
      /// </summary>
      public static readonly Guid EmploymentContractKind = Guid.Parse("ff44444d-e584-4164-ae9c-adc2cc206196");
      
      /// <summary>
      /// Guid вида документа Дополнительное соглашение к трудовому договору.
      /// </summary>
      public static readonly Guid AdditionalAgreementKind = Guid.Parse("2be92e41-6cbe-436e-8cf8-f4ec44ae9179");
      
      /// <summary>
      /// Guid вида документа Соглашение о расторжении трудового договора.
      /// </summary>
      public static readonly Guid AgreementOnTerminationKind = Guid.Parse("6167ec8b-6fbf-437f-a56b-a9249c3f0ff7");
      
      /// <summary>
      /// Guid вида документа Договор о полной индивидуальной материальной ответственности.
      /// </summary>
      public static readonly Guid IndividualLiabilityKind = Guid.Parse("6ba9821d-921a-4b5e-874a-2304926b9bab");
      
      /// <summary>
      /// Guid вида документа Ученический договор.
      /// </summary>
      public static readonly Guid ApprenticeshipAgreementKind = Guid.Parse("11e02bac-18dd-4a94-b13f-e920ae466d16");
      
      /// <summary>
      /// Guid вида документа Соглашение о дополнительном профессиональном образовании.
      /// </summary>
      public static readonly Guid AgreementOnContProfEducationKind = Guid.Parse("303fc460-a2a6-4de7-ac41-c7b47e8a9837");
      
      /// <summary>
      /// Guid вида документа Соглашение NDA.
      /// </summary>
      public static readonly Guid NDAAgreementKind = Guid.Parse("64eae0ee-4139-4876-b2d2-08d2c09fb72c");
      
      /// <summary>
      /// Guid вида документа Соглашение об использовании личного имущества.
      /// </summary>
      public static readonly Guid PersonalPropertyUseAgreementKind = Guid.Parse("539eeb11-3734-4aca-a574-d20dfe3323e0");
      
      /// <summary>
      /// Guid вида документа Соглашение о возмещении ущерба с рассрочкой платежа.
      /// </summary>
      public static readonly Guid IndemnificationWithPaymentByInstallmentsKind = Guid.Parse("b87d1fd6-9d7a-4b3c-96e2-c66236bda549");
      
      /// <summary>
      /// Guid вида документа Соглашение о возмещении причиненного вреда коллективом (бригадой).
      /// </summary>
      public static readonly Guid AgreementOnDamageCompensationByTeamKind = Guid.Parse("84ee0689-ff94-40ed-b04a-d13496658254");
      
      /// <summary>
      /// Guid вида документа Прочий договор.
      /// </summary>
      public static readonly Guid OtherContractKind = Guid.Parse("2724879b-3fa7-4eb8-b2af-620c189fd2d5");
      
      /// <summary>
      /// Guid вида документа Прочее соглашение.
      /// </summary>
      public static readonly Guid OtherAgreementKind = Guid.Parse("fc492ab0-357c-4ef4-bca0-6df6355bead5");
      #endregion
      
      #region Кадровые документы.
      /// <summary>
      /// Guid вида документа Запрос согласия работника на направление в служебную командировку с одновременным уведомлением о праве отказаться от направления в служебную командировку.
      /// </summary>
      public static readonly Guid BusinessTripConsentRequestKind = Guid.Parse("813471d7-ff5f-4c30-b733-b1879adbf53a");
      
      /// <summary>
      /// Guid вида документа Извещение работника, принимаемого на подземные работы о времени и месте проведения проверки его знаний и умений.
      /// </summary>
      public static readonly Guid UndergroundWorkNoticeKind = Guid.Parse("73e14f6a-4bd6-4c15-83e3-46e5f6d67690");
      
      /// <summary>
      /// Guid вида документа Личная карточка работника.
      /// </summary>
      public static readonly Guid EmployeePersonalCardKind = Guid.Parse("ab3418c4-f9ca-41d6-8bff-2d3d0da49c8b");
      
      /// <summary>
      /// Guid вида документа Предложение работнику другой работы при его увольнении в связи с сокращением численности или штата организации.
      /// </summary>
      public static readonly Guid OfferingUponReductionKind = Guid.Parse("c8b648b4-709e-4e5b-8ed9-fa91284e1047");
      
      /// <summary>
      /// Guid вида документа Предложение работнику о переводе на другую работу по медицинским показаниям.
      /// </summary>
      public static readonly Guid MedicalReasonsTransferOfferKind = Guid.Parse("e904acc6-51f4-4041-bb0c-f81e434de384");
      
      /// <summary>
      /// Guid вида документа Предложение другой работы при приостановлении специального права.
      /// </summary>
      public static readonly Guid SpecRightsStopTransferOfferKind = Guid.Parse("f5cbae19-c07c-44fe-bf87-cceda71e0cd5");
      
      /// <summary>
      /// Guid вида документа Предложение другой работы при восстановлении на работе работника, ранее выполнявшего эту работу, по решению государственной инспекции труда или суда.
      /// </summary>
      public static readonly Guid CoutrStateTransferOfferKind = Guid.Parse("dc52dbf8-7154-4ce8-bbed-633069efbd0a");
      
      /// <summary>
      /// Guid вида документа Предложение другой работы при дисквалификации или ином административном наказании, исключающем возможность исполнения работником обязанностей по трудовому договору.
      /// </summary>
      public static readonly Guid DisqualificationTransferOfferKind = Guid.Parse("6717c092-fa4e-4dc5-85c5-c101d4e1d63a");
      
      /// <summary>
      /// Guid вида документа Предложение другой работы при прекращении допуска к государственной тайне, если выполняемая работа требует такого допуска.
      /// </summary>
      public static readonly Guid StateSecretsTerminationTransferOfferKind = Guid.Parse("40878c28-0780-43e0-9759-614edc4de4ed");
      
      /// <summary>
      /// Guid вида документа Предложение другой работы при возникновении ограничений на занятие определенными видами трудовой деятельности, исключающих возможность исполнения работником обязанностей по трудовому договору.
      /// </summary>
      public static readonly Guid RestrictionsOfLaborActivityTransferOfferKind = Guid.Parse("ccc69932-eb08-45af-b062-c3a009215f28");
      
      /// <summary>
      /// Guid вида документа Предложение работнику о переводе на вакантные должности в связи с отказом от продолжения работы в новых условиях.
      /// </summary>
      public static readonly Guid NewConditionsRefuceTransferOfferKind = Guid.Parse("7b170f29-b5ca-4af6-8e30-5444db3278a1");
      
      /// <summary>
      /// Guid вида документа Предложение руководителю о временном переводе на другую работу по медицинским показаниям.
      /// </summary>
      public static readonly Guid TempMedicalReasonsTransferOfferKind = Guid.Parse("d9a9dfb8-7861-4f90-8d74-2354449ee792");
      
      /// <summary>
      /// Guid вида документа Предложение работнику о выполнении дополнительной работы.
      /// </summary>
      public static readonly Guid AdditionalWorkOfferKind = Guid.Parse("ee3cd78a-b2ae-4a36-bcdc-ffd84a647751");
      
      /// <summary>
      /// Guid вида документа Расчетный листок.
      /// </summary>
      public static readonly Guid PayrollKind = Guid.Parse("91711f4c-5b56-44cd-af41-0c9d1ffae73d");
      
      /// <summary>
      /// Guid вида документа Согласие на обработку персональных данных.
      /// </summary>
      public static readonly Guid PersonalDataProcessingConsentKind = Guid.Parse("e7cab151-db7d-4c9b-90bb-b4fe218fd268");
      
      /// <summary>
      /// Guid вида документа Согласие работника на перевод на другую работу.
      /// </summary>
      public static readonly Guid ConsentToTransferKind = Guid.Parse("9e70551c-c4a6-4919-ba1e-953b04e23857");
      
      /// <summary>
      /// Guid вида документа Согласие работника на увольнение до истечения срока предупреждения об увольнении в связи с сокращением численности или штата или в связи с ликвидацией.
      /// </summary>
      public static readonly Guid DismissialBeforeNoticeDueConsentKind = Guid.Parse("58133ae3-02c7-40c0-8983-e8529cc1f39e");
      
      /// <summary>
      /// Guid вида документа Согласие работника на поручение ему дополнительной работы.
      /// </summary>
      public static readonly Guid AddWorkConsentKind = Guid.Parse("faca0044-316c-427d-acb8-5710b32f9805");
      
      /// <summary>
      /// Guid вида документа Согласие работника на отзыв из отпуска.
      /// </summary>
      public static readonly Guid VacationRecallConsentKind = Guid.Parse("91c3d775-4e24-408d-9d55-da64fa490bc3");
      
      /// <summary>
      /// Guid вида документа Требование о даче письменных объяснений работником причин совершения им дисциплинарного проступка.
      /// </summary>
      public static readonly Guid WrittenExplanationsRequirementKind = Guid.Parse("a692ba4a-444c-4c5d-a1d8-9b77500c2e97");
      
      /// <summary>
      /// Guid вида документа Уведомление об увольнении в связи с сокращением численности или штата организации или в связи с ликвидацией организации.
      /// </summary>
      public static readonly Guid LiquidationDismissalNoticeKind = Guid.Parse("34bf618e-56f5-45c9-a3a9-d0d5c9f47b86");
      
      /// <summary>
      /// Guid вида документа Уведомление работодателя об отмене дополнительной работы (по ч 4 ст602 ТК РФ).
      /// </summary>
      public static readonly Guid CancelAdditionalWorkNoticeKind = Guid.Parse("682c53b3-e0df-4f50-9c5b-992fb94ebbee");
      
      /// <summary>
      /// Guid вида документа Уведомление о расторжении трудового договора в связи со сменой собственника.
      /// </summary>
      public static readonly Guid OwnershipChangeRefuseNoticeKind = Guid.Parse("89d7ba01-2056-45b2-91da-3b63aaa38b59");
      
      /// <summary>
      /// Guid вида документа Уведомление работника о его направлении в спортивную сборную команду Российской Федерации.
      /// </summary>
      public static readonly Guid SportTeamAssignmentNoticeKind = Guid.Parse("f7c0f60b-e7cc-4348-9d4d-6b4064e5b3bf");
      
      /// <summary>
      /// Guid вида документа Уведомление работника об изменении условий трудового договора в связи с изменением организационных или технологических условий труда.
      /// </summary>
      public static readonly Guid WorkingConditionsChangeNoticeKind = Guid.Parse("5b26ef6b-9ec2-476f-802a-cbfc3edbb57a");
      
      /// <summary>
      /// Guid вида документа Уведомление выборного органа первичной профсоюзной организации о принятом решении сократить численность или штат работников.
      /// </summary>
      public static readonly Guid TradeUnionLiquidationDismissalNoticeKind = Guid.Parse("268967d7-c547-4a7e-931d-27613bc76247");
      
      /// <summary>
      /// Guid вида документа Уведомление работника об увольнении в связи с истечением срока трудового договора.
      /// </summary>
      public static readonly Guid DueExpirationDismissalNoticeKind = Guid.Parse("c2069622-a9ab-4f18-b96b-48718935e5a3");
      
      /// <summary>
      /// Guid вида документа Уведомление иностранного работника об увольнении в соответствии с п. 10 или п. 11 ч. 1 ст. 327.6 ТК РФ.
      /// </summary>
      public static readonly Guid ForeignEmpDismissialNoticeKind = Guid.Parse("ee1d2b81-29ef-4674-86bb-2ef29ec359fb");
      
      /// <summary>
      /// Guid вида документа Уведомление об увольнении в связи с неудовлетворительным результатом испытания.
      /// </summary>
      public static readonly Guid UnsatisfactoryTestResultDismissialNoticeKind = Guid.Parse("19414f1b-c32e-4dea-a313-244e0c27040d");
      
      /// <summary>
      /// Guid вида документа Уведомление о прекращении трудового договора совместителя в связи с приемом работника, для которого работа будет основной.
      /// </summary>
      public static readonly Guid PartTimeContractTerminationNoticeKind = Guid.Parse("31ee69bb-2212-472a-989f-a8b456ac260e");
      
      /// <summary>
      /// Guid вида документа Уведомление работника религиозной организации о расторжении трудового договора по основаниям, предусмотренным трудовым договором.
      /// </summary>
      public static readonly Guid ReligiousOrganizationTerminationNoticeKind = Guid.Parse("382aee9d-4616-4422-89b5-5cd5f5bfc9a3");
      
      /// <summary>
      /// Guid вида документа Уведомление работника о привлечении к работе в ночное время и о праве отказа от такой работы.
      /// </summary>
      public static readonly Guid NightWorkNoticeKind = Guid.Parse("4b61526d-77c2-465c-b31e-55ab1e9f5e38");
      
      /// <summary>
      /// Guid вида документа Уведомление работника о привлечении к сверхурочной работе и о праве отказа от такой работы.
      /// </summary>
      public static readonly Guid OvertimeWorkNoticeKind = Guid.Parse("255ae5f6-c48f-4062-a634-48fe8f43652c");
      
      /// <summary>
      /// Guid вида документа Уведомление работника о привлечении к работе в выходной или нерабочий праздничный день и о праве отказа от такой работы.
      /// </summary>
      public static readonly Guid WeekendWorkNoticeKind = Guid.Parse("ac56aef8-a9e7-4bf7-905e-acc43b6639ca");
      
      /// <summary>
      /// Guid вида документа Уведомление работника о начале отпуска.
      /// </summary>
      public static readonly Guid VacationNoticeKind = Guid.Parse("e3758655-8fb3-4cac-b110-03f2a22a8f2d");
      
      /// <summary>
      /// Guid вида документа Уведомление работника о выплате задержанной заработной платы.
      /// </summary>
      public static readonly Guid DalayPaymentNoticeKind = Guid.Parse("4b226b65-20d5-4f07-871f-ee6c26755740");
      
      /// <summary>
      /// Guid вида документа Уведомление о даче объяснений причин нанесения материального ущерба работодателю.
      /// </summary>
      public static readonly Guid MaterialDamageExplanationsNoticeKind = Guid.Parse("99903088-4e0e-4b25-8ff3-fe03b37629b9");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о поощрении работника.
      /// </summary>
      public static readonly Guid EncouragementOrderKind = Guid.Parse("091a1cc4-c6ef-4576-a9e7-784ff1953c93");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о поощрении работников.
      /// </summary>
      public static readonly Guid EncouragementOrdersKind = Guid.Parse("e5fd5ca6-fa6d-444e-b1bd-a1df3cd933d9");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о приеме работника на работу.
      /// </summary>
      public static readonly Guid EmploymentOrderKind = Guid.Parse("be04cf83-88c1-4d2e-a385-1806243bcf5b");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о приеме работников на работу.
      /// </summary>
      public static readonly Guid EmploymentOrdersKind = Guid.Parse("ce137dbc-900e-47e8-8e8f-eb15103048c4");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о привлечении к работе за пределами нормальной продолжительности рабочего времени при работе в режиме ненормированного рабочего дня.
      /// </summary>
      public static readonly Guid IrregularWorkingDayOrderKind = Guid.Parse("1ca3b80f-e5e0-4f64-9dd1-362c48c53cae");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о привлечении работника к работе в выходной или нерабочий праздничный день.
      /// </summary>
      public static readonly Guid WeekendWorkOrderKind = Guid.Parse("515547a3-b96c-4447-87ea-ffec3ca38dea");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о применении дисциплинарного взыскания.
      /// </summary>
      public static readonly Guid SanctionOrderKind = Guid.Parse("5006543e-f7e4-41fd-9742-88561946fe98");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о снятии дисциплинарного взыскания.
      /// </summary>
      public static readonly Guid Kind = Guid.Parse("9a3bfcb2-6166-4518-8ca6-a02a4331e6d2");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о снятии дисциплинарного взыскания.
      /// </summary>
      public static readonly Guid LiftingSanctionOrderKind = Guid.Parse("d99ae600-2007-4961-9a62-2436291f5007");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о привлечении работника к материальной ответственности.
      /// </summary>
      public static readonly Guid FinancialLiabilityOrderKind = Guid.Parse("b9bd5642-39d3-49f6-97f7-baa3966e1c73");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о проведении специальной оценки условий труда и формировании комиссии по ее проведению.
      /// </summary>
      public static readonly Guid WorkingConditionsAssessmentOrderKind = Guid.Parse("cb37248d-e3ff-404e-aa0c-717554e653d0");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об утверждении перечня рабочих мест, на которых будет проводиться специальная оценка условий труда.
      /// </summary>
      public static readonly Guid WorkingConditionsAssessmentListOrderKind = Guid.Parse("1aac1ff9-125a-4833-8bd7-3dd753f3d6f0");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о создании комиссии по охране труда и о проведении проверки знаний требований охраны труда работников.
      /// </summary>
      public static readonly Guid LaborProtectionCommissionOrderKind = Guid.Parse("0ffb40f3-77ef-4235-9936-2f3d35aea2e4");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о создании комиссии по расследованию несчастного случая.
      /// </summary>
      public static readonly Guid EstablishmentCommissionOrderKind = Guid.Parse("5262f9fb-51bd-4344-942d-ccbc828559e1");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о предоставлении дополнительного дня отдыха донору.
      /// </summary>
      public static readonly Guid DonorRestDayOrderKind = Guid.Parse("40f73e57-a204-45e5-b332-e1d3f557d669");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о переводе работника на другую работу.
      /// </summary>
      public static readonly Guid TransferOrderKind = Guid.Parse("da6b2b5e-ba0d-4345-9066-7fde33d0d0cf");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о переводе работников на другую работу.
      /// </summary>
      public static readonly Guid TransferOrdersKind = Guid.Parse("7eb1ee6b-ab91-4032-8256-1a4f75faa591");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о перемещении работника.
      /// </summary>
      public static readonly Guid MovementOrderKind = Guid.Parse("5f127664-67de-4758-9d83-7ee3ebe6c22a");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об отстранении работника от работы.
      /// </summary>
      public static readonly Guid SuspensionOrderKind = Guid.Parse("ccbcf598-24b7-44e1-9a4f-b9b658b731d8");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о допуске работника к работе после отстранения.
      /// </summary>
      public static readonly Guid AdmissionAfterSuspensionOrderKind = Guid.Parse("0e1be00b-4d83-4d79-8cba-8ad1b09ff18e");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о поручении дополнительной работы.
      /// </summary>
      public static readonly Guid AdditionalWorkOrderKind = Guid.Parse("55c8fdb6-980d-40dc-a1e1-2194b1cba79f");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об отмене дополнительной работы.
      /// </summary>
      public static readonly Guid CancelAdditionalWorkOrderKind = Guid.Parse("1077c533-5c44-42e3-96e4-3f95f565a454");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о прекращении (расторжении) трудового договора с работником (увольнении).
      /// </summary>
      public static readonly Guid ContractTerminationOrderKind = Guid.Parse("4b2f3a66-d777-4227-9086-11fee6b2a8f3");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о прекращении (расторжении) трудового договора с работниками (увольнении).
      /// </summary>
      public static readonly Guid ContractTerminationOrdersKind = Guid.Parse("22f31e06-16bc-40c0-ac49-9d3fbcac976c");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об отмене приказа об увольнении при восстановлении работника.
      /// </summary>
      public static readonly Guid ReinstatedOrderKind = Guid.Parse("3944e2f3-dab9-4f7a-bcd8-c0b66fb8db7d");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об удержании из заработной платы при увольнении за неотработанные дни отпуска.
      /// </summary>
      public static readonly Guid UnworkedVacationDeductionOrderKind = Guid.Parse("5f9d2043-784c-4a31-a823-98ec596f7e72");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об установлении неполного рабочего времени структурным подразделениям.
      /// </summary>
      public static readonly Guid PartTimeDayOrderKind = Guid.Parse("e571f0fc-8486-455e-a99c-828bb632075e");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о досрочной отмене неполного рабочего времени.
      /// </summary>
      public static readonly Guid CancelPartTimeDayOrderKind = Guid.Parse("12538941-ec51-430f-b802-0ad7f06201ad");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о предоставлении дополнительного времени отдыха за переработку в предпраздничный день.
      /// </summary>
      public static readonly Guid AddRestTimeForPreHolidayDayOrderKind = Guid.Parse("3168f68f-2fde-47b2-a82d-6053c2dbbd78");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о привлечении работника к сверхурочной работе.
      /// </summary>
      public static readonly Guid OvertimeWorkOrderKind = Guid.Parse("eec4ea41-d896-4e66-bee0-da7cb6080194");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о предоставлении дополнительного дня отдыха за работу в выходные или нерабочие праздничные дни.
      /// </summary>
      public static readonly Guid AddRestDayForWorkOnWeekendsOrderKind = Guid.Parse("6551d1e2-2226-4f46-b40e-cd4c777ece91");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о предоставлении отпуска работнику.
      /// </summary>
      public static readonly Guid VacationOrderKind = Guid.Parse("e7047f16-1440-41fc-9094-c54db511bbca");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о предоставлении отпуска работникам.
      /// </summary>
      public static readonly Guid VacationOrdersKind = Guid.Parse("cd5c59b2-a924-4621-90d2-bb474cf174eb");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о переносе отпуска.
      /// </summary>
      public static readonly Guid ShiftVacationOrderKind = Guid.Parse("4fe00617-cad3-4fc1-b01c-a4adcd697f4b");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об отзыве работника из отпуска.
      /// </summary>
      public static readonly Guid RecallVacationOrderKind = Guid.Parse("15fc6f46-79f2-403b-9209-b53a26d68fd4");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о предоставлении работнику, совмещающему работу с получением образования, дополнительного (учебного) отпуска.
      /// </summary>
      public static readonly Guid EducationVacationOrderKind = Guid.Parse("94e426a4-0d40-47d0-957f-0ed0d2f258e8");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о предоставлении отпуска по беременности и родам.
      /// </summary>
      public static readonly Guid MaternityVacationOrderKind = Guid.Parse("56634f2b-1506-454b-8976-0cd72d37b552");
      
       /// <summary>
      /// Guid вида документа Приказ о прекращении отпуска в связи с уходом в отпуск по беременности и родам.
      /// </summary>
      public static readonly Guid MaternityVacationTerminationOrderKind = Guid.Parse("aa302cc7-58af-450c-9054-bda4b6e227f2");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о предоставлении отпуска по уходу за ребенком.
      /// </summary>
      public static readonly Guid ParentalVacationOrderKind = Guid.Parse("8255af8e-264c-482e-9509-8f645956b17d");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о досрочном выходе на работу из отпуска по уходу за ребенком.
      /// </summary>
      public static readonly Guid EarlyWorkFromParentalVacationOrderKind = Guid.Parse("25716265-1fe0-4838-8819-4a8b408be3d7");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о выполнении работы на дому в период отпуска по уходу за ребенком.
      /// </summary>
      public static readonly Guid WorkAtHomeParentalVacationOrderKind = Guid.Parse("73d90eba-da3d-4d91-af55-12d706e96e53");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о предоставлении дополнительного времени отдыха за сверхурочную работу.
      /// </summary>
      public static readonly Guid AddRestForOvertimeOrderKind = Guid.Parse("431771cf-a9d8-4e71-ad0d-9afc59aeea5e");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об удержании из заработной платы.
      /// </summary>
      public static readonly Guid DeductionFromPaymentOrderKind = Guid.Parse("02fe0c27-22b3-4280-8920-6f35a0c4a8a6");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о направлении работника в командировку.
      /// </summary>
      public static readonly Guid BusinessTripOrderKind = Guid.Parse("040f8959-d34c-402b-a614-828861ab48c3");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о направлении работников в командировку.
      /// </summary>
      public static readonly Guid BusinessTripOrdersKind = Guid.Parse("fba79f93-6376-4075-b4db-b9ff42cc1fc7");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о продлении срока командировки.
      /// </summary>
      public static readonly Guid BusinessTripExtensionOrderKind = Guid.Parse("cd4e016c-33c7-4dca-98fe-1952112212ab");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об отзыве работника из командировки.
      /// </summary>
      public static readonly Guid BusinessTripRecallOrderKind = Guid.Parse("2747a582-db26-4d84-afcd-f9c05a9a69d2");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об освобождении от работы в связи со сдачей крови и связанным с этим медобследованием.
      /// </summary>
      public static readonly Guid DonorReleaseFromWorkOrderKind = Guid.Parse("0d7e36da-2cdb-4d5a-9348-e9fba1c44e17");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о возмещении расходов, связанных с использованием личного имущества.
      /// </summary>
      public static readonly Guid PersonalPropertyUseReimbursementOrderKind = Guid.Parse("2600218f-96aa-4d0e-9b1f-fff924ee11bf");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о возмещении расходов при переезде на работу в другую местность.
      /// </summary>
      public static readonly Guid MovingReimbursementOrderKind = Guid.Parse("143b8438-ab51-4558-9948-23cae19aca57");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о выплате работнику денежной компенсации за задержку заработной платы.
      /// </summary>
      public static readonly Guid CompensationForDelayedPaymentOrderKind = Guid.Parse("90b35815-77ef-48a6-b444-7f33963804c2");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об утверждении графика проведения специальной оценки условий труда.
      /// </summary>
      public static readonly Guid ScheduleForSpecialWorkConditionAssessmentOrderKind = Guid.Parse("cbd4a36f-af59-4843-bb13-968488b5cea6");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о создании службы охраны труда.
      /// </summary>
      public static readonly Guid LaborProtectionServiceCreationOrderKind = Guid.Parse("06f00cd4-0d3a-4ecf-a297-1aa0e748ed59");
      
      /// <summary>
      /// Guid вида документа Приказ о возложении обязанностей по охране труда.
      /// </summary>
      public static readonly Guid LaborProtectionDutiesOrderKind = Guid.Parse("32b91d30-a9f2-44fe-bdcd-c486794257ac");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) о предоставлении длительного отпуска педагогическому работнику.
      /// </summary>
      public static readonly Guid LongVacationForTeacherOrderKind = Guid.Parse("9a6a7265-e710-48ce-963c-2dd9c007ba24");
      
      /// <summary>
      /// Guid вида документа Приказ об объявлении простоя.
      /// </summary>
      public static readonly Guid DowntimeOrderKind = Guid.Parse("c38df3bd-5521-4c35-aca8-f2dc710b860d");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об утверждении перечня профессий и должностей работников, освобожденных от прохождения первичного инструктажа на рабочем месте.
      /// </summary>
      public static readonly Guid PrimaryInstructionExecptedOrderKind = Guid.Parse("360fce4b-4db6-4630-9a14-97919023c670");
      
      /// <summary>
      /// Guid вида документа Прочий запрос.
      /// </summary>
      public static readonly Guid OtherRequestKind = Guid.Parse("fbd015d2-3d84-4719-a7c3-f531e93e51fb");
      
      /// <summary>
      /// Guid вида документа Прочее заявление.
      /// </summary>
      public static readonly Guid OtherStatementKind = Guid.Parse("e43426a1-f31d-4944-9bfc-465102c06f89");
      
      /// <summary>
      /// Guid вида документа Прочее извещение.
      /// </summary>
      public static readonly Guid OtherAnnouncementKind = Guid.Parse("32498302-aa7a-4a50-b91d-e513eca2e46d");
      
      /// <summary>
      /// Guid вида документа Прочее предложение.
      /// </summary>
      public static readonly Guid OtherOfferKind = Guid.Parse("34f0080b-8b1f-472b-a362-8dad3bc49d64");
      
      /// <summary>
      /// Guid вида документа Прочий приказ.
      /// </summary>
      public static readonly Guid OtherOrderKind = Guid.Parse("9424d5bb-71bd-4c92-bf14-c24a16395cac");
      
      /// <summary>
      /// Guid вида документа Прочее распоряжение.
      /// </summary>
      public static readonly Guid OtherDirectionKind = Guid.Parse("12130feb-b30b-4b6d-b906-14a5194a3a83");
      
      /// <summary>
      /// Guid вида документа Прочее согласие.
      /// </summary>
      public static readonly Guid OtherConsentKind = Guid.Parse("4a7202cc-16e4-4e80-976c-340745bc1ca2");
      
      /// <summary>
      /// Guid вида документа Прочее требование.
      /// </summary>
      public static readonly Guid OtherRequirementKind = Guid.Parse("e4635e31-253d-49ff-bc3c-358dc78efe14");
      
      /// <summary>
      /// Guid вида документа Прочее уведомление.
      /// </summary>
      public static readonly Guid OtherNoticeKind = Guid.Parse("8b04cca5-311a-46ff-a2e9-b3c40be77a2f");
      
      /// <summary>
      /// Guid вида документа График работы в выходной (нерабочий праздничный) день.
      /// </summary>
      public static readonly Guid WeekendWorkScheduleKind = Guid.Parse("b98ce094-2316-4197-8c21-ab66679b3a8b");
      
      /// <summary>
      /// Guid вида документа График сверхурочной работы.
      /// </summary>
      public static readonly Guid OvertimeScheduleKind = Guid.Parse("2d095dd6-4974-4838-9ccf-ea0f6610a434");
      
      /// <summary>
      /// Guid вида документа Приказ об изменении графика работы.
      /// </summary>
      public static readonly Guid ChangeWorkScheduleOrderKind = Guid.Parse("06338a72-4712-40c7-bee7-e0f7b98021de");
      
      /// <summary>
      /// Guid вида документа Приказ о совмещении должностей.
      /// </summary>
      public static readonly Guid ConcurrentJobsOrderKind = Guid.Parse("d6460996-5c7a-4447-80d2-7c04cae24ed1");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об оплате по среднему заработку.
      /// </summary>
      public static readonly Guid PayByAverageWageOrderKind = Guid.Parse("1ae43747-5ee1-4a45-9367-739b6e48c550");
      
      /// <summary>
      /// Guid вида документа Приказ (распоряжение) об оплате по среднему заработку списком.
      /// </summary>
      public static readonly Guid PayByAverageWageOrdersKind = Guid.Parse("5b311959-d394-45ca-9e9c-07ed57503d83");
      #endregion
      
      #region Локально-нормативные акты.
      /// <summary>
      /// Guid вида документа Правила внутреннего трудового распорядка.
      /// </summary>
      public static readonly Guid InternalLaborRegulationsKind = Guid.Parse("d9ca95e5-7417-417b-99e6-8af89c54ce7a");
      
      /// <summary>
      /// Guid вида документа Положение о хранении и использовании персональных данных работников.
      /// </summary>
      public static readonly Guid PersonalDataStorageRegulationKind = Guid.Parse("f24d9a5f-9004-4152-a489-9bec1292efbf");
      
      /// <summary>
      /// Guid вида документа График сменности.
      /// </summary>
      public static readonly Guid ShiftScheduleKind = Guid.Parse("7db786f0-79ea-4a9d-bdba-2352d7b80ecf");
      
      /// <summary>
      /// Guid вида документа График отпусков.
      /// </summary>
      public static readonly Guid VacationScheduleKind = Guid.Parse("ab4f82a8-a1ad-4b4f-b386-e28c24a892d5");
      
      /// <summary>
      /// Guid вида документа Табель учета рабочего времени.
      /// </summary>
      public static readonly Guid TimeSheetKind = Guid.Parse("075abe8e-5a14-4f51-bd42-b05d690ddc44");
      
      /// <summary>
      /// Guid вида документа Приказ об утверждении локального нормативного акта.
      /// </summary>
      public static readonly Guid LNAOrderKind = Guid.Parse("4a7cf760-b2ad-4913-90c3-48a08f94a356");
      
      /// <summary>
      /// Guid вида документа Штатное расписание.
      /// </summary>
      public static readonly Guid StaffingTableKind = Guid.Parse("7833ca54-d2ef-43f3-aef0-86242319248e");
      
      /// <summary>
      /// Guid вида документа Положение о КЭДО.
      /// </summary>
      public static readonly Guid DocumentManagementRegulationsKind = Guid.Parse("5247e499-a9d9-4219-b0fc-a75aede1ea27");
      
      /// <summary>
      /// Guid вида документа Прочий локальный нормативный акт.
      /// </summary>
      public static readonly Guid OtherLNAKind = Guid.Parse("191727e4-35d3-4684-96cb-edc7ae9daf0c");
      #endregion      
      
      #region Заявления сотрудников.
      /// <summary>
      /// Guid вида документа Согласие на кадровый электронный документооборот.
      /// </summary>
      [Public]
      public static readonly Guid DocumentManagementAgreementKind = Guid.Parse("524788c9-435c-4785-8c83-234d1158ff85");
      
      /// <summary>
      /// Guid вида документа Прочее заявление.
      /// </summary>
      public static readonly Guid OtherStatementRXKind = Guid.Parse("4844bb27-0aeb-4bcd-b96f-801f692e220e");

      /// <summary>
      /// Guid вида документа Заявление на изменение персональных данных.
      /// </summary>
      public static readonly Guid ChangePersonalDataStatementKind = Guid.Parse("5fc9e3c9-d7ea-453e-b544-1865bc1bc220");

      /// <summary>
      /// Guid вида документа Заявление о переводе.
      /// </summary>
      public static readonly Guid TransferStatementKind = Guid.Parse("e8839220-d967-42a2-8c73-7f4dc0c9dac5");

      /// <summary>
      /// Guid вида документа Заявление о переносе отпуска.
      /// </summary>
      public static readonly Guid VacationChangePeriodStatementKind = Guid.Parse("6f477447-b6ce-4e03-893e-3d793cbf42f7");

      /// <summary>
      /// Guid вида документа Заявление о предоставлении дополнительного дня отдыха в связи со сдачей крови.
      /// </summary>
      public static readonly Guid DonorAdditionalRestDayStatementKind = Guid.Parse("1f5ad1dc-0578-4116-8fc7-e91aca03e0e7");

      /// <summary>
      /// Guid вида документа Заявление о предоставлении ежегодного оплачиваемого отпуска.
      /// </summary>
      public static readonly Guid AnnualPaidVacationStatementKind = Guid.Parse("e8c1de5d-6db8-4e68-bd27-cecc5df1104b");

      /// <summary>
      /// Guid вида документа Заявление о предоставлении отпуска без сохранения заработной платы.
      /// </summary>
      public static readonly Guid VacationWithoutPayStatementKind = Guid.Parse("66e03ad5-c723-4495-af3e-5ade627bd96f");

      /// <summary>
      /// Guid вида документа Заявление о предоставлении отпуска по беременности и родам.
      /// </summary>
      public static readonly Guid MaternityLeaveStatementKind = Guid.Parse("f2859c7b-8433-485a-8ad2-63ddd98b00d7");

      /// <summary>
      /// Guid вида документа Заявление о предоставлении учебного отпуска.
      /// </summary>
      public static readonly Guid StudyLeaveStatementKind = Guid.Parse("e190fb5c-10e4-4315-bf79-d61b3e906470");

      /// <summary>
      /// Guid вида документа Заявление об оказании материальной помощи.
      /// </summary>
      public static readonly Guid FinancialAssistanceStatementKind = Guid.Parse("78d0ca90-6511-465d-aa45-42b31a874fee");

      /// <summary>
      /// Guid вида документа Заявление об увольнении.
      /// </summary>
      public static readonly Guid DismissalStatementKind = Guid.Parse("47a32d83-fa98-4c29-a665-4337084321ef");

      #endregion
    }
    
    // Guid ролей.
    [Public]
    public static class Roles
    {
      /// <summary>
      /// Guid роли Кадровый специалист.
      /// </summary>
      [Public]
      public static readonly Guid HRSpecialist = Guid.Parse("09733f4c-799d-4257-a7ea-18ad3d4d8e0b");
    }

    /// <summary>
    /// GUID прав.
    /// HACK копипаста аналогичной константы из EssPlatform, т.к. там константа не публичная.
    /// </summary>
    public static class EssPlatformDefaultAccessRightsTypeSid
    {
      /// <summary>
      /// Приглашение в ЛК.
      /// </summary>
      public static readonly Guid CreateEssUsers = Guid.Parse("d7df6c99-027e-4ef0-8a05-6ef3b57ab88f");
    }
    
    // Листы документы Excel для импорта оргструктуры.
    public static class ExcelImportWorksheetNames
    {
      /// <summary>
      /// Лист "Наши организации".
      /// </summary>
      public const string BusinessUnits = "НашиОрганизации";
      
      /// <summary>
      /// Лист "Подразделения".
      /// </summary>
      public const string Departments = "Подразделения";
      
      /// <summary>
      /// Лист "Сотрудники".
      /// </summary>
      public const string Employees = "Сотрудники";
    }
    
    /// <summary>
    /// Допустимое количество попыток импорта оргструктуры. После превышения пишем ошибку.
    /// </summary>
    public const int ImportOrgstructurePossibleRetryCount = 2;
    
    /// <summary>
    /// Допустимое количество попыток отправить увдомление об импорте оргструктуры. После превышения пишем ошибку.
    /// </summary>
    public const int SendNoticePossibleRetryCount = 2;
    
    /// <summary>
    /// Допустимое количество попыток создания замещений. После превышения пишем ошибку.
    /// </summary>
    public const int CreateSubstitutionsForEmployeeRetryCount = 5;
    
    /// <summary>
    /// Guid типов задач.
    /// </summary>
    public static class TaskTypeGuid
    {
      /// <summary>
      /// Guid задачи на ознакомление с кадровыми документами и ЛНА.
      /// </summary>
      [Public]
      public static readonly Guid AcquaintanceType = Guid.Parse("4cdd4742-2b9e-4b92-8c5b-9f5f451279d5");
      
      /// <summary>
      /// Guid задачи на подписание кадровых договорных документов.
      /// </summary>
      [Public]
      public static readonly Guid SignType = Guid.Parse("95857521-3ca3-422e-b4fc-cdb725669771");
      
    }
    
    /// <summary>
    /// Ключи параметров Docflow.
    /// </summary>
    public static class ParamKey
    {
      /// <summary>
      /// Ключ периода мониторинга задач на планирование в подразделениях, в секундах.
      /// </summary>
      [Public]
      public const string AcquaintanceMonitoringTimeKey = "DirRX.HRSolution.AcquaintanceMonitoringTime";
      
      /// <summary>
      /// Ключ периода мониторинга задач на планирование в подразделениях, в секундах.
      /// </summary>
      [Public]
      public const string SignMonitoringTimeKey = "DirRX.HRSolution.SignMonitoringTime";
    }
    
    // Операция "Подписание соглашения на ЭДО" для записи в историю справочника Сотрудники.
    [Public]
    public const string SignAgreement = "SignAgreement";
  }
  
}