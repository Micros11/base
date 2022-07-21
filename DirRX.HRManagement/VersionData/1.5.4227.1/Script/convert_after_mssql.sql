-- Заполнить свойство Старт в групее Согласование справочника Настройка согласования и подписания.
if exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_NAME = 'DirRX_HRManag_TaskSetting' AND COLUMN_NAME = 'ApprovalQueue')
begin 
  update
    DirRX_HRManag_TaskSetting
  set
    ApprovalQueue = 'Parallel'
  where
    ApprovalQueue is null
    and Exists (select Id from DirRX_HRManag_Approvers where TaskSetting = DirRX_HRManag_TaskSetting.Id)
end