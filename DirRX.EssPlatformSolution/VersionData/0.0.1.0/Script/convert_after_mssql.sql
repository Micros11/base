-- Записать свойства, связанные с работой с ЛК из временной таблицы в таблицу справочника Сотрудники - для конвертации с версии 1.0.
if exists (SELECT *
           FROM information_schema.tables
           WHERE table_name = 'EssPlatform_EmployeeData_temp')
   and exists (SELECT *
               FROM information_schema.COLUMNS
               WHERE table_name = 'Sungero_Core_Recipient' and COLUMN_NAME = 'PersAccStatus_EssPlS_DirRX')
begin
  UPDATE
    r
  SET
    PersAccStatus_EssPlS_DirRX = tmp.PersAccStatus,
    PersonalPhone_EssPlS_DirRX = tmp.PersonalPhone,
    PersonalEmail_EssPlS_DirRX = tmp.PersonalEmail
  FROM EssPlatform_EmployeeData_temp tmp
    join Sungero_Core_Recipient r on tmp.Id = r.Id
  DROP table EssPlatform_EmployeeData_temp
end

-- Заполнить новое свойство в справочнике Сотрудники - для первичной установки. 
if exists (SELECT * 
           FROM information_schema.COLUMNS 
           WHERE table_name = 'Sungero_Core_Recipient' 
                 and COLUMN_NAME = 'PersAccStatus_EssPlS_DirRX')
begin
  -- Для всех записей, где еще не заполнено свойство статус регистарции личного кабинета, задать значение по умолчанию.
  UPDATE Sungero_Core_Recipient
  SET PersAccStatus_EssPlS_DirRX = 'InviteIsNotSent'        
  WHERE PersAccStatus_EssPlS_DirRX is NULL and Discriminator = 'B7905516-2BE5-4931-961C-CB38D5677565' and Status = 'Active'
end