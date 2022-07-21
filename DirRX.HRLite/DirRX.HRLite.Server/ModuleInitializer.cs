using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Company;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;
using DocflowInitFunctions = Sungero.Docflow.PublicInitializationFunctions.Module;
using DocKind = DirRX.HRLite.Constants.Module.DocumentKind;
using HRLiteRoles = DirRX.HRLite.Constants.Module.Roles;

namespace DirRX.HRLite.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      this.CreateDocumentTypes();
      this.CreateDocumentKinds();
      this.CreateRoles();
      this.GrantRights();
      CreateReportsTables();
      CreateTaskSettings();
    }
    
    #region Создание видов и типов документов.

    /// <summary>
    /// Создать типы документов.
    /// </summary>
    public virtual void CreateDocumentTypes()
    {
      InitializationLogger.Debug("Init: Create document types.");
      var innerDocumentFlow = Sungero.Docflow.DocumentRegister.DocumentFlow.Inner;
      // Кадровые документы.
      DocflowInitFunctions.CreateDocumentType(Resources.PersonnelDocumentTypeName, PersonnelDocument.ClassTypeGuid, innerDocumentFlow, true);
      // Кадровые договорные документы.
      DocflowInitFunctions.CreateDocumentType(Resources.BilaterialDocumentTypeName, BilaterialDocument.ClassTypeGuid, innerDocumentFlow, true);
      // Локально-нормативные акты.
      DocflowInitFunctions.CreateDocumentType(Resources.LocalRegulationDocumentTypeName, LocalRegulationDocument.ClassTypeGuid, innerDocumentFlow, true);
      // Заявления сотрудников.
      DocflowInitFunctions.CreateDocumentType(Resources.StatementDocumentTypename, StatementDocument.ClassTypeGuid, innerDocumentFlow, true);
    }

    /// <summary>
    /// Создать виды документов.
    /// </summary>
    public virtual void CreateDocumentKinds()
    {
      InitializationLogger.Debug("Init: Create document kinds.");

      var notNumerable = Sungero.Docflow.DocumentKind.NumberingType.NotNumerable;
      var innerDocumentFlow = Sungero.Docflow.DocumentRegister.DocumentFlow.Inner;
      
      #region Кадровые договорные документы.
      // Трудовой договор.
      DocflowInitFunctions.CreateDocumentKind(Resources.EmploymentContract, Resources.EmploymentContract,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.EmploymentContractKind, false);
      
      // Дополнительное соглашение к трудовому договору.
      DocflowInitFunctions.CreateDocumentKind(Resources.AdditionalAgreement, Resources.AdditionalAgreementShort,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.AdditionalAgreementKind, true);
      
      // Соглашение о расторжении трудового договора.
      DocflowInitFunctions.CreateDocumentKind(Resources.AgreementOnTermination, Resources.Agreement,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.AgreementOnTerminationKind, false);
      
      // Договор о полной индивидуальной материальной ответственности.
      DocflowInitFunctions.CreateDocumentKind(Resources.IndividualLiability, Resources.Contract,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.IndividualLiabilityKind, false);
      
      // Ученический договор.
      DocflowInitFunctions.CreateDocumentKind(Resources.ApprenticeshipAgreement, Resources.ApprenticeshipAgreementShort,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.ApprenticeshipAgreementKind, false);
      
      // Соглашение о дополнительном профессиональном образовании.
      DocflowInitFunctions.CreateDocumentKind(Resources.AgreementOnContProfEducation, Resources.Agreement,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.AgreementOnContProfEducationKind, false);
      
      // Соглашение NDA.
      DocflowInitFunctions.CreateDocumentKind(Resources.NDAAgreement, Resources.Agreement,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.NDAAgreementKind, false);
      
      // Соглашение об использовании личного имущества.
      DocflowInitFunctions.CreateDocumentKind(Resources.PersonalPropertyUseAgreement, Resources.Agreement,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.PersonalPropertyUseAgreementKind, false);
      
      // Соглашение о возмещении ущерба с рассрочкой платежа.
      DocflowInitFunctions.CreateDocumentKind(Resources.IndemnificationWithPaymentByInstallments, Resources.Agreement,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.IndemnificationWithPaymentByInstallmentsKind, false);
      
      // Соглашение о возмещении причиненного вреда коллективом (бригадой).
      DocflowInitFunctions.CreateDocumentKind(Resources.AgreementOnDamageCompensationByTeam, Resources.Agreement,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.AgreementOnDamageCompensationByTeamKind, false);
      
      // Прочий договор.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherContract, Resources.Contract,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.OtherContractKind, false);
      
      // Прочее соглашение.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherAgreement, Resources.Agreement,
                                              notNumerable, innerDocumentFlow, true, false, BilaterialDocument.ClassTypeGuid, null, DocKind.OtherAgreementKind, false);
      #endregion
      
      #region Кадровые документы.
      // Запрос согласия работника на направление в служебную командировку с одновременным уведомлением о праве отказаться от направления в служебную командировку.
      DocflowInitFunctions.CreateDocumentKind(Resources.BusinessTripConsentRequest, Resources.ConsentRequest,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.BusinessTripConsentRequestKind, false);
      
      // Извещение работника, принимаемого на подземные работы о времени и месте проведения проверки его знаний и умений.
      DocflowInitFunctions.CreateDocumentKind(Resources.UndergroundWorkNotice, Resources.Announcement,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.UndergroundWorkNoticeKind, false);
      
      // Личная карточка работника.
      DocflowInitFunctions.CreateDocumentKind(Resources.EmployeePersonalCard, Resources.EmployeePersonalCard,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.EmployeePersonalCardKind, true);
      
      // Предложение работнику другой работы при его увольнении в связи с сокращением численности или штата организации.
      DocflowInitFunctions.CreateDocumentKind(Resources.OfferingUponReduction, Resources.Offer,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OfferingUponReductionKind, false);
      
      // Предложение работнику о переводе на другую работу по медицинским показаниям.
      DocflowInitFunctions.CreateDocumentKind(Resources.MedicalReasonsTransferOffer, Resources.Offer,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.MedicalReasonsTransferOfferKind, false);
      
      // Предложение другой работы при приостановлении специального права.
      DocflowInitFunctions.CreateDocumentKind(Resources.SpecRightsStopTransferOffer, Resources.Offer,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.SpecRightsStopTransferOfferKind, false);
      
      // Предложение другой работы при восстановлении на работе работника, ранее выполнявшего эту работу, по решению государственной инспекции труда или суда.
      DocflowInitFunctions.CreateDocumentKind(Resources.CoutrStateTransferOffer, Resources.Offer,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.CoutrStateTransferOfferKind, false);
      
      // Предложение другой работы при дисквалификации или ином административном наказании, исключающем возможность исполнения работником обязанностей по трудовому договору.
      DocflowInitFunctions.CreateDocumentKind(Resources.DisqualificationTransferOffer, Resources.Offer,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.DisqualificationTransferOfferKind, false);
      
      // Предложение другой работы при прекращении допуска к государственной тайне, если выполняемая работа требует такого допуска.
      DocflowInitFunctions.CreateDocumentKind(Resources.StateSecretsTerminationTransferOffer, Resources.Offer,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.StateSecretsTerminationTransferOfferKind, false);
      
      // Предложение другой работы при возникновении ограничений на занятие определенными видами трудовой деятельности, исключающих возможность исполнения работником обязанностей по трудовому договору.
      DocflowInitFunctions.CreateDocumentKind(Resources.RestrictionsOfLaborActivityTransferOffer, Resources.Offer,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.RestrictionsOfLaborActivityTransferOfferKind, false);
      
      // Предложение работнику о переводе на вакантные должности в связи с отказом от продолжения работы в новых условиях.
      DocflowInitFunctions.CreateDocumentKind(Resources.NewConditionsRefuceTransferOffer, Resources.Offer,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.NewConditionsRefuceTransferOfferKind, false);
      
      // Предложение руководителю о временном переводе на другую работу по медицинским показаниям.
      DocflowInitFunctions.CreateDocumentKind(Resources.TempMedicalReasonsTransferOffer, Resources.Offer,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.TempMedicalReasonsTransferOfferKind, false);
      
      // Предложение работнику о выполнении дополнительной работы.
      DocflowInitFunctions.CreateDocumentKind(Resources.AdditionalWorkOffer, Resources.Offer,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.AdditionalWorkOfferKind, false);
      
      // Расчетный листок.
      DocflowInitFunctions.CreateDocumentKind(Resources.Payroll, Resources.Payroll,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.PayrollKind, false);
      
      // Согласие на обработку персональных данных.
      DocflowInitFunctions.CreateDocumentKind(Resources.PersonalDataProcessingConsent, Resources.PersonalDataProcessingConsent,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.PersonalDataProcessingConsentKind, false);
      
      // Согласие работника на перевод на другую работу.
      DocflowInitFunctions.CreateDocumentKind(Resources.ConsentToTransfer, Resources.Consent,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.ConsentToTransferKind, false);
      
      // Согласие работника на увольнение до истечения срока предупреждения об увольнении в связи с сокращением численности или штата или в связи с ликвидацией.
      DocflowInitFunctions.CreateDocumentKind(Resources.DismissialBeforeNoticeDueConsent, Resources.Consent,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.DismissialBeforeNoticeDueConsentKind, false);
      
      // Согласие работника на поручение ему дополнительной работы.
      DocflowInitFunctions.CreateDocumentKind(Resources.AddWorkConsent, Resources.Consent,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.AddWorkConsentKind, false);
      
      // Согласие работника на отзыв из отпуска.
      DocflowInitFunctions.CreateDocumentKind(Resources.VacationRecallConsent, Resources.Consent,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.VacationRecallConsentKind, false);
      
      // Требование о даче письменных объяснений работником причин совершения им дисциплинарного проступка.
      DocflowInitFunctions.CreateDocumentKind(Resources.WrittenExplanationsRequirement, Resources.WrittenExplanationsRequirementShort,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.WrittenExplanationsRequirementKind, false);
      
      // Уведомление об увольнении в связи с сокращением численности или штата организации или в связи с ликвидацией организации.
      DocflowInitFunctions.CreateDocumentKind(Resources.LiquidationDismissalNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.LiquidationDismissalNoticeKind, false);
      
      // Уведомление работодателя об отмене дополнительной работы (по ч 4 ст602 ТК РФ).
      DocflowInitFunctions.CreateDocumentKind(Resources.CancelAdditionalWorkNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.CancelAdditionalWorkNoticeKind, false);
      
      // Уведомление о расторжении трудового договора в связи со сменой собственника.
      DocflowInitFunctions.CreateDocumentKind(Resources.OwnershipChangeRefuseNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OwnershipChangeRefuseNoticeKind, false);
      
      // Уведомление работника о его направлении в спортивную сборную команду Российской Федерации.
      DocflowInitFunctions.CreateDocumentKind(Resources.SportTeamAssignmentNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.SportTeamAssignmentNoticeKind, false);
      
      // Уведомление работника об изменении условий трудового договора в связи с изменением организационных или технологических условий труда.
      DocflowInitFunctions.CreateDocumentKind(Resources.WorkingConditionsChangeNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.WorkingConditionsChangeNoticeKind, false);
      
      // Уведомление выборного органа первичной профсоюзной организации о принятом решении сократить численность или штат работников.
      DocflowInitFunctions.CreateDocumentKind(Resources.TradeUnionLiquidationDismissalNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.TradeUnionLiquidationDismissalNoticeKind, false);
      
      // Уведомление работника об увольнении в связи с истечением срока трудового договора.
      DocflowInitFunctions.CreateDocumentKind(Resources.DueExpirationDismissalNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.DueExpirationDismissalNoticeKind, false);
      
      // Уведомление иностранного работника об увольнении в соответствии с п. 10 или п. 11 ч. 1 ст. 327.6 ТК РФ.
      DocflowInitFunctions.CreateDocumentKind(Resources.ForeignEmpDismissialNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.ForeignEmpDismissialNoticeKind, false);
      
      // Уведомление об увольнении в связи с неудовлетворительным результатом испытания.
      DocflowInitFunctions.CreateDocumentKind(Resources.UnsatisfactoryTestResultDismissialNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.UnsatisfactoryTestResultDismissialNoticeKind, false);
      
      // Уведомление о прекращении трудового договора совместителя в связи с приемом работника, для которого работа будет основной.
      DocflowInitFunctions.CreateDocumentKind(Resources.PartTimeContractTerminationNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.PartTimeContractTerminationNoticeKind, false);
      
      // Уведомление работника религиозной организации о расторжении трудового договора по основаниям, предусмотренным трудовым договором.
      DocflowInitFunctions.CreateDocumentKind(Resources.ReligiousOrganizationTerminationNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.ReligiousOrganizationTerminationNoticeKind, false);
      
      // Уведомление работника о привлечении к работе в ночное время и о праве отказа от такой работы.
      DocflowInitFunctions.CreateDocumentKind(Resources.NightWorkNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.NightWorkNoticeKind, false);
      
      // Уведомление работника о привлечении к сверхурочной работе и о праве отказа от такой работы.
      DocflowInitFunctions.CreateDocumentKind(Resources.OvertimeWorkNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OvertimeWorkNoticeKind, false);
      
      // Уведомление работника о привлечении к работе в выходной или нерабочий праздничный день и о праве отказа от такой работы.
      DocflowInitFunctions.CreateDocumentKind(Resources.WeekendWorkNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.WeekendWorkNoticeKind, false);
      
      // Уведомление работника о начале отпуска.
      DocflowInitFunctions.CreateDocumentKind(Resources.VacationNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.VacationNoticeKind, false);
      
      // Уведомление работника о выплате задержанной заработной платы.
      DocflowInitFunctions.CreateDocumentKind(Resources.DalayPaymentNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.DalayPaymentNoticeKind, false);
      
      // Уведомление о даче объяснений причин нанесения материального ущерба работодателю.
      DocflowInitFunctions.CreateDocumentKind(Resources.MaterialDamageExplanationsNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.MaterialDamageExplanationsNoticeKind, false);
      
      // Приказ (распоряжение) о поощрении работника.
      DocflowInitFunctions.CreateDocumentKind(Resources.EncouragementOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.EncouragementOrderKind, false);
      
      // Приказ (распоряжение) о поощрении работников.
      DocflowInitFunctions.CreateDocumentKind(Resources.EncouragementOrders, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.EncouragementOrdersKind, false);
      
      // Приказ (распоряжение) о приеме работника на работу.
      DocflowInitFunctions.CreateDocumentKind(Resources.EmploymentOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.EmploymentOrderKind, false);
      
      // Приказ (распоряжение) о приеме работников на работу.
      DocflowInitFunctions.CreateDocumentKind(Resources.EmploymentOrders, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.EmploymentOrdersKind, false);
      
      // Приказ (распоряжение) о привлечении к работе за пределами нормальной продолжительности рабочего времени при работе в режиме ненормированного рабочего дня.
      DocflowInitFunctions.CreateDocumentKind(Resources.IrregularWorkingDayOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.IrregularWorkingDayOrderKind, false);
      
      // Приказ (распоряжение) о привлечении работника к работе в выходной или нерабочий праздничный день.
      DocflowInitFunctions.CreateDocumentKind(Resources.WeekendWorkOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.WeekendWorkOrderKind, false);
      
      // Приказ (распоряжение) о применении дисциплинарного взыскания.
      DocflowInitFunctions.CreateDocumentKind(Resources.SanctionOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.SanctionOrderKind, false);
      
      // Приказ (распоряжение) о снятии дисциплинарного взыскания.
      DocflowInitFunctions.CreateDocumentKind(Resources.LiftingSanctionOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.LiftingSanctionOrderKind, false);
      
      // Приказ (распоряжение) о привлечении работника к материальной ответственности.
      DocflowInitFunctions.CreateDocumentKind(Resources.FinancialLiabilityOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.FinancialLiabilityOrderKind, false);
      
      // Приказ (распоряжение) о проведении специальной оценки условий труда и формировании комиссии по ее проведению.
      DocflowInitFunctions.CreateDocumentKind(Resources.WorkingConditionsAssessmentOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.WorkingConditionsAssessmentOrderKind, false);
      
      // Приказ (распоряжение) об утверждении перечня рабочих мест, на которых будет проводиться специальная оценка условий труда.
      DocflowInitFunctions.CreateDocumentKind(Resources.WorkingConditionsAssessmentListOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.WorkingConditionsAssessmentListOrderKind, false);
      
      // Приказ (распоряжение) о создании комиссии по охране труда и о проведении проверки знаний требований охраны труда работников.
      DocflowInitFunctions.CreateDocumentKind(Resources.LaborProtectionCommissionOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.LaborProtectionCommissionOrderKind, false);
      
      // Приказ (распоряжение) о создании комиссии по расследованию несчастного случая.
      DocflowInitFunctions.CreateDocumentKind(Resources.EstablishmentCommissionOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.EstablishmentCommissionOrderKind, false);
      
      // Приказ (распоряжение) о предоставлении дополнительного дня отдыха донору.
      DocflowInitFunctions.CreateDocumentKind(Resources.DonorRestDayOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.DonorRestDayOrderKind, false);
      
      // Приказ (распоряжение) о переводе работника на другую работу.
      DocflowInitFunctions.CreateDocumentKind(Resources.TransferOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.TransferOrderKind, false);
      
      // Приказ (распоряжение) о переводе работников на другую работу.
      DocflowInitFunctions.CreateDocumentKind(Resources.TransferOrders, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.TransferOrdersKind, false);
      
      // Приказ (распоряжение) о перемещении работника.
      DocflowInitFunctions.CreateDocumentKind(Resources.MovementOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.MovementOrderKind, false);
      
      // Приказ (распоряжение) об отстранении работника от работы.
      DocflowInitFunctions.CreateDocumentKind(Resources.SuspensionOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.SuspensionOrderKind, false);
      
      // Приказ (распоряжение) о допуске работника к работе после отстранения.
      DocflowInitFunctions.CreateDocumentKind(Resources.AdmissionAfterSuspensionOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.AdmissionAfterSuspensionOrderKind, false);
      
      // Приказ (распоряжение) о поручении дополнительной работы.
      DocflowInitFunctions.CreateDocumentKind(Resources.AdditionalWorkOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.AdditionalWorkOrderKind, false);
      
      // Приказ (распоряжение) об отмене дополнительной работы.
      DocflowInitFunctions.CreateDocumentKind(Resources.CancelAdditionalWorkOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.CancelAdditionalWorkOrderKind, false);
      
      // Приказ (распоряжение) о прекращении (расторжении) трудового договора с работником (увольнении).
      DocflowInitFunctions.CreateDocumentKind(Resources.ContractTerminationOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.ContractTerminationOrderKind, false);
      
      // Приказ (распоряжение) о прекращении (расторжении) трудового договора с работниками (увольнении).
      DocflowInitFunctions.CreateDocumentKind(Resources.ContractTerminationOrders, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.ContractTerminationOrdersKind, false);
      
      // Приказ (распоряжение) об отмене приказа об увольнении при восстановлении работника.
      DocflowInitFunctions.CreateDocumentKind(Resources.ReinstatedOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.ReinstatedOrderKind, false);
      
      // Приказ (распоряжение) об удержании из заработной платы при увольнении за неотработанные дни отпуска.
      DocflowInitFunctions.CreateDocumentKind(Resources.UnworkedVacationDeductionOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.UnworkedVacationDeductionOrderKind, false);
      
      // Приказ (распоряжение) об установлении неполного рабочего времени структурным подразделениям.
      DocflowInitFunctions.CreateDocumentKind(Resources.PartTimeDayOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.PartTimeDayOrderKind, false);
      
      // Приказ (распоряжение) о досрочной отмене неполного рабочего времени.
      DocflowInitFunctions.CreateDocumentKind(Resources.CancelPartTimeDayOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.CancelPartTimeDayOrderKind, false);
      
      // Приказ (распоряжение) о предоставлении дополнительного времени отдыха за переработку в предпраздничный день.
      DocflowInitFunctions.CreateDocumentKind(Resources.AddRestTimeForPreHolidayDayOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.AddRestTimeForPreHolidayDayOrderKind, false);
      
      // Приказ (распоряжение) о привлечении работника к сверхурочной работе.
      DocflowInitFunctions.CreateDocumentKind(Resources.OvertimeWorkOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OvertimeWorkOrderKind, false);
      
      // Приказ (распоряжение) о предоставлении дополнительного дня отдыха за работу в выходные или нерабочие праздничные дни.
      DocflowInitFunctions.CreateDocumentKind(Resources.DdRestDayForWorkOnWeekendsOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.AddRestDayForWorkOnWeekendsOrderKind, false);
      
      // Приказ (распоряжение) о предоставлении отпуска работнику.
      DocflowInitFunctions.CreateDocumentKind(Resources.VacationOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.VacationOrderKind, false);
      
      // Приказ (распоряжение) о предоставлении отпуска работникам.
      DocflowInitFunctions.CreateDocumentKind(Resources.VacationOrders, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.VacationOrdersKind, false);
      
      // Приказ (распоряжение) о переносе отпуска.
      DocflowInitFunctions.CreateDocumentKind(Resources.ShiftVacationOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.ShiftVacationOrderKind, false);
      
      // Приказ (распоряжение) об отзыве работника из отпуска.
      DocflowInitFunctions.CreateDocumentKind(Resources.RecallVacationOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.RecallVacationOrderKind, false);
      
      // Приказ (распоряжение) о предоставлении работнику, совмещающему работу с получением образования, дополнительного (учебного) отпуска.
      DocflowInitFunctions.CreateDocumentKind(Resources.EducationVacationOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.EducationVacationOrderKind, false);
      
      // Приказ (распоряжение) о предоставлении отпуска по беременности и родам.
      DocflowInitFunctions.CreateDocumentKind(Resources.MaternityVacationOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.MaternityVacationOrderKind, false);
      
      // Приказ о прекращении отпуска в связи с уходом в отпуск по беременности и родам.
      DocflowInitFunctions.CreateDocumentKind(Resources.MaternityVacationTerminationOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.MaternityVacationTerminationOrderKind, false);
      
      // Приказ (распоряжение) о предоставлении отпуска по уходу за ребенком.
      DocflowInitFunctions.CreateDocumentKind(Resources.ParentalVacationOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.ParentalVacationOrderKind, false);
      
      // Приказ (распоряжение) о досрочном выходе на работу из отпуска по уходу за ребенком.
      DocflowInitFunctions.CreateDocumentKind(Resources.EarlyWorkFromParentalVacationOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.EarlyWorkFromParentalVacationOrderKind, false);
      
      // Приказ (распоряжение) о выполнении работы на дому в период отпуска по уходу за ребенком.
      DocflowInitFunctions.CreateDocumentKind(Resources.WorkAtHomeParentalVacationOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.WorkAtHomeParentalVacationOrderKind, false);
      
      // Приказ (распоряжение) о предоставлении дополнительного времени отдыха за сверхурочную работу.
      DocflowInitFunctions.CreateDocumentKind(Resources.AddRestForOvertimeOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.AddRestForOvertimeOrderKind, false);
      
      // Приказ (распоряжение) об удержании из заработной платы.
      DocflowInitFunctions.CreateDocumentKind(Resources.DeductionFromPaymentOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.DeductionFromPaymentOrderKind, false);
      
      // Приказ (распоряжение) о направлении работника в командировку.
      DocflowInitFunctions.CreateDocumentKind(Resources.BusinessTripOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.BusinessTripOrderKind, false);
      
      // Приказ (распоряжение) о направлении работников в командировку
      DocflowInitFunctions.CreateDocumentKind(Resources.BusinessTripOrders, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.BusinessTripOrdersKind, false);
      
      // Приказ (распоряжение) о продлении срока командировки.
      DocflowInitFunctions.CreateDocumentKind(Resources.BusinessTripExtensionOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.BusinessTripExtensionOrderKind, false);
      
      // Приказ (распоряжение) об отзыве работника из командировки.
      DocflowInitFunctions.CreateDocumentKind(Resources.BusinessTripRecallOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.BusinessTripRecallOrderKind, false);
      
      // Приказ (распоряжение) об освобождении от работы в связи со сдачей крови и связанным с этим медобследованием.
      DocflowInitFunctions.CreateDocumentKind(Resources.DonorReleaseFromWorkOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.DonorReleaseFromWorkOrderKind, false);
      
      // Приказ (распоряжение) о возмещении расходов, связанных с использованием личного имущества.
      DocflowInitFunctions.CreateDocumentKind(Resources.PersonalPropertyUseReimbursementOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.PersonalPropertyUseReimbursementOrderKind, false);
      
      // Приказ (распоряжение) о возмещении расходов при переезде на работу в другую местность.
      DocflowInitFunctions.CreateDocumentKind(Resources.MovingReimbursementOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.MovingReimbursementOrderKind, false);
      
      // Приказ (распоряжение) о выплате работнику денежной компенсации за задержку заработной платы.
      DocflowInitFunctions.CreateDocumentKind(Resources.CompensationForDelayedPaymentOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.CompensationForDelayedPaymentOrderKind, false);
      
      // Приказ (распоряжение) об утверждении графика проведения специальной оценки условий труда.
      DocflowInitFunctions.CreateDocumentKind(Resources.ScheduleForSpecialWorkConditionAssessmentOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.ScheduleForSpecialWorkConditionAssessmentOrderKind, false);
      
      // Приказ (распоряжение) о создании службы охраны труда.
      DocflowInitFunctions.CreateDocumentKind(Resources.LaborProtectionServiceCreationOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.LaborProtectionServiceCreationOrderKind, false);
      
      // Приказ о возложении обязанностей по охране труда.
      DocflowInitFunctions.CreateDocumentKind(Resources.LaborProtectionDutiesOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.LaborProtectionDutiesOrderKind, false);
      
      // Приказ (распоряжение) о предоставлении длительного отпуска педагогическому работнику.
      DocflowInitFunctions.CreateDocumentKind(Resources.LongVacationForTeacherOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.LongVacationForTeacherOrderKind, false);
      
      // Приказ об объявлении простоя.
      DocflowInitFunctions.CreateDocumentKind(Resources.DowntimeOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.DowntimeOrderKind, false);
      
      // Приказ (распоряжение) об утверждении перечня профессий и должностей работников, освобожденных от прохождения первичного инструктажа на рабочем месте.
      DocflowInitFunctions.CreateDocumentKind(Resources.PrimaryInstructionExecptedOrder, Resources.PrimaryInstructionExecptedOrder,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.PrimaryInstructionExecptedOrderKind, false);
      
      // Прочий запрос.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherRequest, Resources.Request,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OtherRequestKind, false);
      
      // Прочее заявление.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherStatement, Resources.Statement,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OtherStatementKind, false);
      
      // Прочее извещение.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherAnnouncement, Resources.Announcement,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OtherAnnouncementKind, false);
      
      // Прочее предложение.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherOffer, Resources.Offer,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OtherOfferKind, false);
      
      // Прочий приказ.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OtherOrderKind, false);
      
      // Прочее распоряжение.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherDirection, Resources.Direction,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OtherDirectionKind, false);
      
      // Прочее согласие.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherConsent, Resources.Consent,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OtherConsentKind, false);
      
      // Прочее требование.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherRequirement, Resources.Requirement,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OtherRequirementKind, false);
      
      // Прочее уведомление.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherNotice, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OtherNoticeKind, false);
      
      // График работы в выходной (нерабочий праздничный) день.
      DocflowInitFunctions.CreateDocumentKind(Resources.WeekendWorkSchedule, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.WeekendWorkScheduleKind, false);
      
      // График сверхурочной работы.
      DocflowInitFunctions.CreateDocumentKind(Resources.OvertimeSchedule, Resources.Notice,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.OvertimeScheduleKind, false);
      
      // Приказ об изменении графика работы.
      DocflowInitFunctions.CreateDocumentKind(Resources.ChangeWorkScheduleOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.ChangeWorkScheduleOrderKind, false);
      
      // Приказ о совмещении должностей.
      DocflowInitFunctions.CreateDocumentKind(Resources.ConcurrentJobsOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.ConcurrentJobsOrderKind, false);
      
      // Приказ (распоряжение) об оплате по среднему заработку.
      DocflowInitFunctions.CreateDocumentKind(Resources.PayByAverageWageOrder, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.PayByAverageWageOrderKind, false);
      
      // Приказ (распоряжение) об оплате по среднему заработку списком.
      DocflowInitFunctions.CreateDocumentKind(Resources.PayByAverageWageOrders, Resources.Order,
                                              notNumerable, innerDocumentFlow, true, false, PersonnelDocument.ClassTypeGuid, null, DocKind.PayByAverageWageOrdersKind, false);
      #endregion
      
      #region Локально-нормативные акты.
      // Правила внутреннего трудового распорядка.
      DocflowInitFunctions.CreateDocumentKind(Resources.InternalLaborRegulations, Resources.InternalLaborRegulations,
                                              notNumerable, innerDocumentFlow, true, false, LocalRegulationDocument.ClassTypeGuid, null, DocKind.InternalLaborRegulationsKind, false);
      
      // Положение о хранении и использовании персональных данных работников.
      DocflowInitFunctions.CreateDocumentKind(Resources.PersonalDataStorageRegulation, Resources.PersonalDataStorageRegulation,
                                              notNumerable, innerDocumentFlow, true, false, LocalRegulationDocument.ClassTypeGuid, null, DocKind.PersonalDataStorageRegulationKind, false);
      
      // График сменности.
      DocflowInitFunctions.CreateDocumentKind(Resources.ShiftSchedule, Resources.ShiftSchedule,
                                              notNumerable, innerDocumentFlow, true, false, LocalRegulationDocument.ClassTypeGuid, null, DocKind.ShiftScheduleKind, false);
      
      // График отпусков.
      DocflowInitFunctions.CreateDocumentKind(Resources.VacationSchedule, Resources.VacationSchedule,
                                              notNumerable, innerDocumentFlow, true, false, LocalRegulationDocument.ClassTypeGuid, null, DocKind.VacationScheduleKind, true);
      
      // Табель учета рабочего времени.
      DocflowInitFunctions.CreateDocumentKind(Resources.TimeSheet, Resources.TimeSheet,
                                              notNumerable, innerDocumentFlow, true, false, LocalRegulationDocument.ClassTypeGuid, null, DocKind.TimeSheetKind, false);
      
      // Приказ об утверждении локального нормативного акта.
      DocflowInitFunctions.CreateDocumentKind(Resources.LNAOrder, Resources.LNAOrderShort,
                                              notNumerable, innerDocumentFlow, true, false, LocalRegulationDocument.ClassTypeGuid, null, DocKind.LNAOrderKind, false);
      
      // Штатное расписание.
      DocflowInitFunctions.CreateDocumentKind(Resources.StaffingTable, Resources.StaffingTable,
                                              notNumerable, innerDocumentFlow, true, false, LocalRegulationDocument.ClassTypeGuid, null, DocKind.StaffingTableKind, false);
      
      // Положение о КЭДО.
      DocflowInitFunctions.CreateDocumentKind(Resources.DocumentManagementRegulationsKind, Resources.DocumentManagementRegulationsKind,
                                              notNumerable, innerDocumentFlow, true, false, LocalRegulationDocument.ClassTypeGuid, null, DocKind.DocumentManagementRegulationsKind, false);
      
      // Прочий локальный нормативный акт.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherLNA, Resources.Act,
                                              notNumerable, innerDocumentFlow, true, false, LocalRegulationDocument.ClassTypeGuid, null, DocKind.OtherLNAKind, false);
      #endregion
      
      #region Заявления сотрудников.
      // Согласие на кадровый электронный документооборот.
      DocflowInitFunctions.CreateDocumentKind(Resources.DocumentManagementAgreementKind, Resources.DocumentManagementAgreementKind,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.DocumentManagementAgreementKind, false);
      
      // Прочее заявление.
      DocflowInitFunctions.CreateDocumentKind(Resources.OtherStatement, Resources.OtherStatement,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.OtherStatementRXKind, false);
      
      // Заявление на изменение персональных данных.
      DocflowInitFunctions.CreateDocumentKind(Resources.ChangePersonalDataStatement, Resources.ChangePersonalDataStatement,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.ChangePersonalDataStatementKind, false);
      
      // Заявление о переводе.
      DocflowInitFunctions.CreateDocumentKind(Resources.TransferStatement, Resources.TransferStatement,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.TransferStatementKind, false);
      
      // Заявление о переносе отпуска.
      DocflowInitFunctions.CreateDocumentKind(Resources.VacationChangePeriodStatement, Resources.VacationChangePeriodStatement,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.VacationChangePeriodStatementKind, false);
      
      // Заявление о предоставлении дополнительного дня отдыха в связи со сдачей крови.
      DocflowInitFunctions.CreateDocumentKind(Resources.DonorAdditionalRestDayStatement, Resources.DonorAdditionalRestDayStatement,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.DonorAdditionalRestDayStatementKind, false);
      
      // Заявление о предоставлении ежегодного оплачиваемого отпуска.
      DocflowInitFunctions.CreateDocumentKind(Resources.AnnualPaidVacationStatement, Resources.AnnualPaidVacationStatement,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.AnnualPaidVacationStatementKind, false);
      
      // Заявление о предоставлении отпуска без сохранения заработной платы.
      DocflowInitFunctions.CreateDocumentKind(Resources.VacationWithoutPayStatement, Resources.VacationWithoutPayStatement,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.VacationWithoutPayStatementKind, false);
      
      // Заявление о предоставлении отпуска по беременности и родам.
      DocflowInitFunctions.CreateDocumentKind(Resources.MaternityLeaveStatement, Resources.MaternityLeaveStatement,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.MaternityLeaveStatementKind, false);
      
      // Заявление о предоставлении учебного отпуска.
      DocflowInitFunctions.CreateDocumentKind(Resources.StudyLeaveStatement, Resources.StudyLeaveStatement,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.StudyLeaveStatementKind, false);
      
      // Заявление об оказании материальной помощи.
      DocflowInitFunctions.CreateDocumentKind(Resources.FinancialAssistanceStatement, Resources.FinancialAssistanceStatement,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.FinancialAssistanceStatementKind, false);
      
      // Заявление об увольнении.
      DocflowInitFunctions.CreateDocumentKind(Resources.DismissalStatement, Resources.DismissalStatement,
                                              notNumerable, innerDocumentFlow, true, false, StatementDocument.ClassTypeGuid, null, DocKind.DismissalStatementKind, false);
      
      #endregion

      
    }

    #endregion
    
    #region Создание ролей и выдача прав.
    /// <summary>
    /// Создать роли.
    /// </summary>
    public virtual void CreateRoles()
    {
      InitializationLogger.Debug("Init: Create roles.");
      DocflowInitFunctions.CreateRole(Resources.HRSpecialistRoleName, Resources.HRSpecialistRoleDescr, HRLiteRoles.HRSpecialist);
      
      var documentHRProUsersRole = Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.HRManagement.Resources.RoleNameHRProUsers, DirRX.HRManagement.Resources.RoleNameHRProUsers, DirRX.HRManagement.PublicConstants.Module.HRRoles.HRProUsers);
      if (!documentHRProUsersRole.RecipientLinks.Any())
      {
        documentHRProUsersRole.RecipientLinks.AddNew().Member = Roles.GetAll(g => g.Sid == DirRX.HRManagement.PublicConstants.Module.HRRoles.AllUsers).FirstOrDefault();
        documentHRProUsersRole
          .Save();
      }
    }
    
    /// <summary>
    /// Выдать права.
    /// </summary>
    public virtual void GrantRights()
    {
      #region Выдача прав всем пользователям.
      var allUsers = Sungero.CoreEntities.Roles.AllUsers;
      if (allUsers != null)
      {
        InitializationLogger.Debug("Init: Grant rights to all users.");
        
        // Справочники.
        EmployeeDocumentLinks.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Change);
        EmployeeDocumentLinks.AccessRights.Save();
        StatementsSettings.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        StatementsSettings.AccessRights.Save();
        StatementTasks.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Create);
        StatementTasks.AccessRights.Save();
        StatementDocuments.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Create);
        StatementDocuments.AccessRights.Save();

      }
      #endregion
      
      #region Выдача прав роли "Кадровый специалист".
      
      var hrSpecialist = ModuleFunctions.GetRole(HRLiteRoles.HRSpecialist);
      if (hrSpecialist != null)
      {
        InitializationLogger.Debug("Init: Grant rights to HR Specialist.");
        
        // Документы.
        BilaterialDocuments.AccessRights.Grant(hrSpecialist, DefaultAccessRightsTypes.Create);
        BilaterialDocuments.AccessRights.Save();
        LocalRegulationDocuments.AccessRights.Grant(hrSpecialist, DefaultAccessRightsTypes.Create);
        LocalRegulationDocuments.AccessRights.Save();
        PersonnelDocuments.AccessRights.Grant(hrSpecialist, DefaultAccessRightsTypes.Create);
        PersonnelDocuments.AccessRights.Save();
        
        // Задачи.
        HRAcquaintanceTasks.AccessRights.Grant(hrSpecialist, DefaultAccessRightsTypes.Create);
        HRAcquaintanceTasks.AccessRights.Save();
        InformingTasks.AccessRights.Grant(hrSpecialist, DefaultAccessRightsTypes.Create);
        InformingTasks.AccessRights.Save();
        SignTasks.AccessRights.Grant(hrSpecialist, DefaultAccessRightsTypes.Create);
        SignTasks.AccessRights.Save();
        ConsentTasks.AccessRights.Grant(hrSpecialist, DefaultAccessRightsTypes.Create);
        ConsentTasks.AccessRights.Save();
        
        // Справочники.
        Employees.AccessRights.Grant(hrSpecialist, DefaultAccessRightsTypes.Change);
        Employees.AccessRights.Grant(hrSpecialist, HRLite.Constants.Module.EssPlatformDefaultAccessRightsTypeSid.CreateEssUsers);
        Employees.AccessRights.Save();
        StatementsSettings.AccessRights.Grant(hrSpecialist, DefaultAccessRightsTypes.Change);
        StatementsSettings.AccessRights.Save();
        
        // Вычисляемые папки.
        SpecialFolders.OnControl.AccessRights.Grant(hrSpecialist, DefaultAccessRightsTypes.Read);
        SpecialFolders.OnControl.AccessRights.Save();
        DirRX.HRLite.SpecialFolders.ActiveTasks.AccessRights.Grant(hrSpecialist, DefaultAccessRightsTypes.Read);
        DirRX.HRLite.SpecialFolders.ActiveTasks.AccessRights.Save();
      }
      #endregion
      
    }
    #endregion
    
    #region Отчеты
    
    /// <summary>
    /// Создать таблицы для отчетов.
    /// </summary>
    public static void CreateReportsTables()
    {
      var acquaintanceControlReportTableName = Constants.AcquaintanceControlReport.SourceTableName;
      Sungero.Docflow.PublicFunctions.Module.DropReportTempTable(acquaintanceControlReportTableName);
      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.AcquaintanceControlReport.CreateAcquaintanceControlReportTable, new[] { acquaintanceControlReportTableName });
    }
    
    #endregion

    #region Настройки задач.
    
    /// <summary>
    /// Создать настройку задачи.
    /// </summary>
    /// <param name="taskInfo">Данные задачи.</param>
    public static void CreateTaskSetting(Sungero.Workflow.ITaskInfo taskInfo)
    {
      if (!HRManagement.TaskSettings.GetAll(s => s.TaskType == taskInfo.Name && s.DocumentKind == null && s.BusinessUnit == null && s.Status == HRManagement.TaskSetting.Status.Active).Any())
      {
        var taskSetting = HRManagement.TaskSettings.Create();
        taskSetting.Name = taskInfo.LocalizedName;
        taskSetting.TaskType = taskInfo.Name;
        taskSetting.SignQueue = HRManagement.TaskSetting.SignQueue.EmployerFirst;
        taskSetting.SignerDealineDays = 3;
        taskSetting.EmployeeDealineDays = 3;
        taskSetting.Save();
      }
    }
    
    /// <summary>
    /// Создать настройки задач.
    /// </summary>
    public static void CreateTaskSettings()
    {
      CreateTaskSetting(SignTasks.Info);
      CreateTaskSetting(HRAcquaintanceTasks.Info);
    }
    #endregion
    
    #region Заполнение ключей параметров Docflow.
    public static void FillDocflowParamKeysWithDefaulValues()
    {
      Sungero.Docflow.PublicFunctions.Module.InsertOrUpdateDocflowParam(HRLite.PublicConstants.Module.ParamKey.AcquaintanceMonitoringTimeKey, Constants.HRAcquaintanceTask.DefaultMonitoringPeriod.ToString());
      Sungero.Docflow.PublicFunctions.Module.InsertOrUpdateDocflowParam(HRLite.PublicConstants.Module.ParamKey.SignMonitoringTimeKey, Constants.SignTask.DefaultMonitoringPeriod.ToString());
    }
    #endregion
  }
}
