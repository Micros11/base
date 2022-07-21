DO $$
begin
-- Заполнить новые свойства в справочнике Сотрудники. 
  if exists (SELECT *
             FROM information_schema.columns
             WHERE table_name = 'sungero_core_recipient' and column_name = 'smsnotifdirrx_esspls_dirrx')
  then
-- Если приглашение в ЛК отправлено - заполнить свойство СМС значением true по умолчанию, если не отправлено - то все в false.
    UPDATE Sungero_Core_Recipient
    SET SmsNotifDirRX_EssPlS_DirRX = 
        CASE 
        WHEN PersAccStatus_EssPlS_DirRX = 'InviteIsNotSent'  THEN true ELSE false 
        END, 
        EmailNotifDirR_EssPlS_DirRX = false, 
        ViberNotifDirR_EssPlS_DirRX = false 
        WHERE SmsNotifDirRX_EssPlS_DirRX is null and Discriminator = 'b7905516-2be5-4931-961c-cb38d5677565' and Status = 'Active'; 
  end if; 
end$$;