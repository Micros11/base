-- Заполнить свойство Согласие на КЭДО в справочнике Сотрудники. 
if exists (SELECT * 
           FROM information_schema.COLUMNS 
           WHERE table_name = 'Sungero_Core_Recipient' 
                 AND COLUMN_NAME = 'ConsentDirRX_HRSolut_DirRX')

  begin
    -- Для сотрудников, которые подключены к ЛК, проставить Подписано, для остальных записей - Не подписано. 
    UPDATE Sungero_Core_Recipient
    SET 
      ConsentDirRX_HRSolut_DirRX = 
        CASE 
          WHEN PersAccStatus_EssPlS_DirRX = 'InviteAccepted' THEN 'Signed' 
          ELSE 'NotSent'
        END
    WHERE Discriminator = 'b7905516-2be5-4931-961c-cb38d5677565'
          and ConsentDirRX_HRSolut_DirRX is NULL;
  end