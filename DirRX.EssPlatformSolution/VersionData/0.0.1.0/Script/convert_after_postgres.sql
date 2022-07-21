DO $$
begin
-- Записать свойства, связанные с работой с ЛК из временной таблицы в таблицу справочника Сотрудники - для конвертации с версии 1.0.
if exists (SELECT *
           FROM information_schema.tables
           WHERE table_name = 'essplatform_employeedata_temp')
   and exists (SELECT *
               FROM information_schema.columns
               WHERE table_name = 'sungero_core_recipient' and column_name = 'persaccstatus_esspls_dirrx')
  then
    UPDATE
      Sungero_Core_Recipient
    SET
      PersAccStatus_EssPlS_DirRX = tmp.PersAccStatus,
      PersonalPhone_EssPlS_DirRX = tmp.PersonalPhone,
      PersonalEmail_EssPlS_DirRX = tmp.PersonalEmail
    FROM EssPlatform_EmployeeData_temp tmp
    WHERE tmp.Id = Sungero_Core_Recipient.Id;
    DROP table EssPlatform_EmployeeData_temp;
  end if;

-- Заполнить новое свойство в справочнике Сотрудники - для первичной установки. 
if exists (SELECT *
           FROM information_schema.columns
           WHERE table_name = 'sungero_core_recipient' and column_name = 'persaccstatus_esspls_dirrx')
  then
    -- Для всех записей, где еще не заполнено свойство статус регистарции личного кабинета, задать значение по умолчанию.
    UPDATE Sungero_Core_Recipient
    SET PersAccStatus_EssPlS_DirRX = 'InviteIsNotSent'
    WHERE PersAccStatus_EssPlS_DirRX IS NULL and Discriminator = 'b7905516-2be5-4931-961c-cb38d5677565' and Status = 'Active';
  end if;
end$$;