DO $$
begin
--Заполнить свойство Person в задаче HiringTask значением из свойства Aspirant.Person из этой же задачи.
	if exists (SELECT * FROM information_schema.columns
              WHERE table_name = 'sungero_wf_task' and column_name = 'person_hrmanag_dirrx')
  then
	 UPDATE Sungero_WF_Task
	 SET Person_HRManag_DirRX = aspirant.Person
	 FROM DirRX_HRManag_Aspirant aspirant
	 WHERE aspirant.id = Sungero_WF_Task.Aspirant1_HRManag_DirRX;
	end if;
end$$;