-- Записать свойства, связанные с работой с ЛК во временную таблицу.
if exists (SELECT *
           FROM information_schema.COLUMNS
           WHERE table_name = 'Sungero_Core_Recipient' and COLUMN_NAME = 'PersAccStatus_HRLiteS_DirRX')
begin
  execute('if exists (SELECT *
                      FROM information_schema.tables
                      WHERE table_name = ''EssPlatform_EmployeeData_temp'')
           begin
             DROP table EssPlatform_EmployeeData_temp
           end
           CREATE table EssPlatform_EmployeeData_temp (Id integer, PersAccStatus nvarchar(15), PersonalPhone nvarchar(250), PersonalEmail nvarchar(250))
           INSERT INTO EssPlatform_EmployeeData_temp
             SELECT r.Id, r.PersAccStatus_HRLiteS_DirRX, r.PersonalPhone_HRLiteS_DirRX, r.PersonalEmail_HRLiteS_DirRX
             FROM Sungero_Core_Recipient r
             WHERE r.Discriminator = ''B7905516-2BE5-4931-961C-CB38D5677565''')
end