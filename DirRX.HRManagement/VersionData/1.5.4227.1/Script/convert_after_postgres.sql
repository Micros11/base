DO $$
begin
  -- Заполнить свойство Старт в групее Согласование справочника Настройка согласования и подписания.
	if exists (SELECT * FROM information_schema.columns
              WHERE table_name = 'dirrx_hrmanag_approvers' and column_name = 'approvalqueue')
  then
	  update
      DirRX_HRManag_TaskSetting
    set
      ApprovalQueue = 'Parallel'
    where
      ApprovalQueue is null
      and Exists (select Id from DirRX_HRManag_Approvers where TaskSetting = DirRX_HRManag_TaskSetting.Id);
	end if;
end$$;