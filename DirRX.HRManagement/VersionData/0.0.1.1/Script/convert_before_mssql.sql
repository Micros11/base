-- TODO Kovalenko Конвертирую отдельными скриптами, т.к конвертация года в задаче на планирование уже в мастере. Потом объединю в один скрипт. 
if exists (SELECT * FROM information_schema.tables 
               WHERE table_name = 'HR_YearInPlanning')
drop table HR_YearInPlanning

if exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'YearP_HRManag_DirRX')
AND (SELECT DATA_TYPE FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'YearP_HRManag_DirRX') = 'int'
AND exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year2_HRManag_DirRX')
AND (SELECT DATA_TYPE FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year2_HRManag_DirRX') = 'int'
begin
  execute('create table HR_YearInPlanning(Id int, year int, yearDep int) ' +  
          'insert into HR_YearInPlanning(id, year, yearDep) ' +
          'select task.Id, task.YearP_HRManag_DirRX, task.Year2_HRManag_DirRX from Sungero_WF_Task as task ' +
          'where task.YearP_HRManag_DirRX is not NULL or task.Year2_HRManag_DirRX is not null')
end

--Сконвертировать поле Год в задаче на планирование отпуска, перенос отпуска, оформление заявления
if exists (SELECT * FROM information_schema.tables 
               WHERE table_name = 'HR_YearInTask')
drop table HR_YearInTask
--TODO Kovalenko Расскоментировать для объединения скрипта в один 
--if exists (SELECT * FROM information_schema.COLUMNS 
--               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'YearP_HRManag_DirRX')
--AND (SELECT DATA_TYPE FROM information_schema.COLUMNS 
--               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'YearP_HRManag_DirRX') = 'int'
--AND exists (SELECT * FROM information_schema.COLUMNS 
--               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year2_HRManag_DirRX')
--AND (SELECT DATA_TYPE FROM information_schema.COLUMNS 
--               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year2_HRManag_DirRX') = 'int'
if exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year_HRManag_DirRX')
AND (SELECT DATA_TYPE FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year_HRManag_DirRX') = 'int'               
AND exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year1_HRManag_DirRX')
AND (SELECT DATA_TYPE FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Task' AND COLUMN_NAME = 'Year1_HRManag_DirRX') = 'int'
--TODO Kovalenko Расскоментировать для объединения скрипта в один 
--begin
--  execute('create table HR_YearInTask(Id int, yearPlan int, yearDep int, yearShift int, yearApproval int) ' +  
--          'insert into HR_YearInTask(id, yearPlan, yearDep, yearShift, yearApproval) ' +
--          'select task.Id, task.YearP_HRManag_DirRX, task.Year2_HRManag_DirRX, task.Year_HRManag_DirRX, task.Year1_HRManag_DirRX from Sungero_WF_Task as task ' +
--          'where ' +
--             'task.YearP_HRManag_DirRX is not NULL or task.Year2_HRManag_DirRX is not NULL or ' +
--             'task.Year_HRManag_DirRX is not NULL or task.Year1_HRManag_DirRX is not NULL')
--end
begin
  execute('create table HR_YearInTask(Id int, yearShift int, yearApproval int) ' +  
          'insert into HR_YearInTask(id, yearShift, yearApproval) ' +
          'select task.Id, task.Year_HRManag_DirRX, task.Year1_HRManag_DirRX from Sungero_WF_Task as task ' +
          'where ' +
             'task.Year_HRManag_DirRX is not NULL or task.Year1_HRManag_DirRX is not NULL')
end

--Сконвертировать поле Год в задании на перенос отпуска 
if exists (SELECT * FROM information_schema.tables 
               WHERE table_name = 'HR_YearInAssignment')
drop table HR_YearInAssignment

if exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Assignment' AND COLUMN_NAME = 'Year_HRManag_DirRX')
AND (SELECT DATA_TYPE FROM information_schema.COLUMNS 
               WHERE table_name = 'Sungero_WF_Assignment' AND COLUMN_NAME = 'Year_HRManag_DirRX') = 'int'
begin
  execute('create table HR_YearInAssignment(Id int, year int) ' +  
          'insert into HR_YearInAssignment(id, year) ' +
          'select assignment.Id, assignment.Year_HRManag_DirRX from Sungero_WF_Assignment as assignment ' +
          'where assignment.Year_HRManag_DirRX is not NULL')
end

--Сконвертировать поле Учетный год в справочнике Отпуска
if exists (SELECT * FROM information_schema.tables 
               WHERE table_name = 'HR_YearInVacation')
drop table HR_YearInVacation

if exists (SELECT * FROM information_schema.COLUMNS 
               WHERE table_name = 'DirRX_HRManag_Vacation' AND COLUMN_NAME = 'Year')
AND (SELECT DATA_TYPE FROM information_schema.COLUMNS 
               WHERE table_name = 'DirRX_HRManag_Vacation' AND COLUMN_NAME = 'Year') = 'int'
begin
  execute('create table HR_YearInVacation(Id int, year int) ' +  
          'insert into HR_YearInVacation(id, year) ' +
          'select vacation.Id, vacation.Year from DirRX_HRManag_Vacation as vacation')
end