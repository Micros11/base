-- TODO Kovalenko Конвертирую отдельными скриптами, т.к конвертация года в задаче на планирование уже в мастере. Потом объединю в один скрипт. 
if exists (SELECT * FROM information_schema.tables
               WHERE table_name = 'HR_YearInPlanning')
AND exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'YearP_HRManag_DirRX')
AND exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year2_HRManag_DirRX')
begin
  UPDATE
      task
  SET
      YearP_HRManag_DirRX = DATEFROMPARTS(tmp.year, 1, 1),
      Year2_HRManag_DirRX = DATEFROMPARTS(tmp.yearDep, 1, 1) 
  FROM 
    HR_YearInPlanning tmp
  JOIN 
    Sungero_WF_Task task on tmp.Id = task.Id

  drop table HR_YearInPlanning
end
--Сконвертировать поле Год в задаче на планирование отпуска, перенос отпуска, оформление заявления
if exists (SELECT * FROM information_schema.tables 
               WHERE table_name = 'HR_YearInTask')
--TODO Kovalenko Расскоментировать для объединения скрипта в один 
--AND exists (SELECT * FROM information_schema.COLUMNS 
--               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'YearP_HRManag_DirRX')
--AND exists (SELECT * FROM information_schema.COLUMNS 
--               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year2_HRManag_DirRX')
AND exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year_HRManag_DirRX')
AND exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year1_HRManag_DirRX')
begin
  UPDATE
      task
  SET
      --TODO Kovalenko Расскоментировать для объединения скрипта в один 
      --YearP_HRManag_DirRX = tmp.yearPlan,--DATEFROMPARTS(tmp.yearPlan, 1, 1),
      --Year2_HRManag_DirRX = tmp.yearDep, --DATEFROMPARTS(tmp.yearDep, 1, 1),
      Year_HRManag_DirRX = 
      case 
        when tmp.yearShift = 0
        then DATEFROMPARTS(1753, 1, 1)
        else DATEFROMPARTS(tmp.yearShift, 1, 1)
      end,
      Year1_HRManag_DirRX = DATEFROMPARTS(tmp.yearApproval, 1, 1)
  FROM 
    HR_YearInTask tmp
  JOIN 
    Sungero_WF_Task task on tmp.Id = task.Id

  drop table HR_YearInTask
end

--Сконвертировать поле Учетный год в справочнике Отпуска
if exists (SELECT * FROM information_schema.tables 
               WHERE table_name = 'HR_YearInVacation')
AND exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'DirRX_HRManag_Vacation' AND COLUMN_NAME = 'Year')
begin
  UPDATE
      vacation
  SET
      vacation.Year = DATEFROMPARTS(tmp.year, 1, 1)
  FROM 
    HR_YearInVacation tmp
  JOIN 
    DirRX_HRManag_Vacation vacation on tmp.Id = vacation.Id

  drop table HR_YearInVacation
end