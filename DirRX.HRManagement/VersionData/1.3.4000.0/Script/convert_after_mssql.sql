-- Заполнить свойство Person в задаче HiringTask значением из свойства Aspirant.Person из этой же задачи. 
if exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'Sungero_WF_Task' AND COLUMN_NAME = 'Person_HRManag_DirRX')
  begin 
    UPDATE task
      SET task.Person_HRManag_DirRX = aspirant.Person
      FROM Sungero_WF_Task task
      JOIN DirRX_HRManag_Aspirant aspirant on task.Aspirant1_HRManag_DirRX = aspirant.id
  end 