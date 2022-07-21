-- Заполнить свойство Year в HRDocument значением учетного года из отпуска. 
if exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_Content_EDoc' AND COLUMN_NAME = 'Year_HRManag_DirRX')

  begin
    -- Заявление в задаче на перенос/отзыв из отпуска(VacationShiftTask). Год берется из задачи.
    UPDATE doc
      SET Year_HRManag_DirRX = task.Year_HRManag_DirRX
      FROM Sungero_WF_Task task
      JOIN Sungero_Content_EDoc doc on task.Statement2_HRManag_DirRX = doc.id 
      WHERE doc.Year_HRManag_DirRX is null
        
    -- Заявление в задаче на согласование отпуска (VacationApprovalTask). Год берется из задачи. 
    UPDATE doc
      SET Year_HRManag_DirRX = task.Year1_HRManag_DirRX
      FROM Sungero_WF_Task task
      JOIN Sungero_Content_EDoc doc on task.Statement4_HRManag_DirRX = doc.id 
      WHERE doc.Year_HRManag_DirRX is null
   
   -- Заявление в задаче на индивидуальное планирование(VacationSingleSchedulingTask). Год берется из задачи.
   UPDATE doc
      SET Year_HRManag_DirRX = task.Year3_HRManag_DirRX
      FROM Sungero_WF_Task task
      JOIN Sungero_Content_EDoc doc on task.Statement3_HRManag_DirRX = doc.id 
      WHERE doc.Year_HRManag_DirRX is null

    -- Уведомление в задаче на подтверждения отпуска (VacationAlertTask). Год берется по цепочке: Уведомление-задача-отпуск-год
    UPDATE doc
      SET Year_HRManag_DirRX = vac.Year
      FROM DirRX_HRManag_VacationAlertT vacations
      JOIN Sungero_WF_Task task on vacations.Task = task.Id
      JOIN Sungero_Content_EDoc doc on doc.id = task.Notification_HRManag_DirRX 
      JOIN DirRX_HRManag_Vacation vac on vacations.Vacation = vac.Id  
      WHERE doc.Year_HRManag_DirRX is null  
  end

-- Заполнить свойство Year1 в HROrder значением учетного года из отпуска. 
if exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_Content_EDoc' AND COLUMN_NAME = 'Year1_HRManag_DirRX')

  begin
    -- Приказ в задаче на перенос/отзыв из отпуска(VacationShiftTask). Год берется из задачи. 
    UPDATE doc
      SET Year1_HRManag_DirRX = task.Year_HRManag_DirRX
      FROM Sungero_WF_Task task
      JOIN Sungero_Content_EDoc doc on task.Order4_HRManag_DirRX = doc.id 
      WHERE doc.Year1_HRManag_DirRX is null
    
    -- Приказ в задаче на согласование отпуска (VacationApprovalTask). Год берется из задачи. 
    UPDATE doc
      SET Year1_HRManag_DirRX = task.Year1_HRManag_DirRX
      FROM Sungero_WF_Task task
      JOIN Sungero_Content_EDoc doc on task.Order3_HRManag_DirRX = doc.id 
      WHERE doc.Year1_HRManag_DirRX is null
    
    -- Приказ на индивидуальное планирование(VacationSingleSchedulingTask). Год берется из задачи.
    UPDATE doc
      SET Year1_HRManag_DirRX = task.Year3_HRManag_DirRX
      FROM Sungero_WF_Task task
      JOIN Sungero_Content_EDoc doc on task.Order5_HRManag_DirRX = doc.id 
      WHERE doc.Year1_HRManag_DirRX is null
  end

-- Заполнить новые свойства в TransferTask и заданиях TransferPrepareDocsAssignment и TransferStatementApprovalAssignment. 
if exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'OldDepartment_HRManag_DirRX')

  begin
    -- Guid задачи "Изменение условий труда"
    declare @TransferTaskGuid NVARCHAR(36);
    set @TransferTaskGuid = '01940621-365A-4767-A824-1E240B2DB781'
    
    -- В задаче TransferTask подразделение взять из записи сотрудника, новые свойства логического типа заполнить по умолчанию. 
    UPDATE task
      SET OldDepartment_HRManag_DirRX = emp.Department_Company_Sungero,
          IsTransfer_HRManag_DirRX = 1,
          SignImm_HRManag_DirRX = 0,
          IsHRStarted_HRManag_DirRX = 0,
          IsManagerStart_HRManag_DirRX = 0
      FROM Sungero_WF_Task task
      JOIN dbo.Sungero_Core_Recipient emp on task.TransfEmpl_HRManag_DirRX = emp.id 
      WHERE task.Discriminator = @TransferTaskGuid and task.OldDepartment_HRManag_DirRX is null
      
    -- В задании TransferPrepareDocsAssignment взять данные из задачи
    UPDATE prepare_assignment
      SET Employee2_HRManag_DirRX = task.TransfEmpl_HRManag_DirRX,
          OldDepartment1_HRManag_DirRX = task.OldDepartment_HRManag_DirRX
      FROM Sungero_WF_Assignment prepare_assignment
      JOIN Sungero_WF_Task task on prepare_assignment.Task = task.id 
      WHERE task.Discriminator = @TransferTaskGuid and prepare_assignment.Employee2_HRManag_DirRX is null
      
    -- В задании TransferStatementApprovalAssignment взять данные из задачи
    UPDATE statement_assignment
      SET Employee1_HRManag_DirRX = task.TransfEmpl_HRManag_DirRX,
          OldDepartment_HRManag_DirRX = task.OldDepartment_HRManag_DirRX,
          IsTransfer_HRManag_DirRX = 1
      FROM Sungero_WF_Assignment statement_assignment
      JOIN Sungero_WF_Task task on statement_assignment.Task = task.id 
      WHERE task.Discriminator = @TransferTaskGuid and statement_assignment.Employee1_HRManag_DirRX is null
  end