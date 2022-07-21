-- Заполнить новые свойства в справочнике Сотрудники. 
if exists (SELECT * 
           FROM information_schema.COLUMNS 
           WHERE table_name = 'Sungero_Core_Recipient' 
                 and COLUMN_NAME = 'SmsNotifDirRX_EssPlS_DirRX')
begin
  -- Если приглашение в ЛК отправлено - заполнить свойство СМС значением true по умолчанию, если не отправлено - то все в false.
  UPDATE Sungero_Core_Recipient
  SET 
  SmsNotifDirRX_EssPlS_DirRX = 
    CASE
      WHEN PersAccStatus_EssPlS_DirRX = 'InviteIsNotSent'  THEN 0 ELSE 1
    END,
  EmailNotifDirR_EssPlS_DirRX = 0,
  ViberNotifDirR_EssPlS_DirRX = 0
  WHERE SmsNotifDirRX_EssPlS_DirRX is null and Discriminator = 'b7905516-2be5-4931-961c-cb38d5677565' and Status = 'Active'
end