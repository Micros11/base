-- Записать свойства, связанные с работой с ЛК во временную таблицу.
DO $$
begin
if exists (SELECT *
           FROM information_schema.columns
           WHERE table_name = 'sungero_core_recipient' and column_name = 'persaccstatus_hrlites_dirrx')
  then
    if exists (SELECT *
               FROM information_schema.tables
               WHERE table_name = 'essplatform_employeedata_temp')
      then
        DROP table EssPlatform_EmployeeData_temp;
      end if;
    CREATE table EssPlatform_EmployeeData_temp (Id int, PersAccStatus varchar(15), PersonalPhone varchar(250), PersonalEmail varchar(250));
    INSERT INTO EssPlatform_EmployeeData_temp
      SELECT r.Id, r.PersAccStatus_HRLiteS_DirRX, r.PersonalPhone_HRLiteS_DirRX, r.PersonalEmail_HRLiteS_DirRX
      FROM Sungero_Core_Recipient r
      WHERE r.Discriminator = 'b7905516-2be5-4931-961c-cb38d5677565';
  end if;
end$$