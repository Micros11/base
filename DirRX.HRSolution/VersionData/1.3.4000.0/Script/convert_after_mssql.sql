-- Заполнить новое свойство в справочнике Сотрудники. 
if exists (SELECT * 
           FROM information_schema.COLUMNS 
           WHERE table_name = 'Sungero_Core_Recipient' 
                 AND COLUMN_NAME = 'SignKindDirRX_HRSolut_DirRX')

  begin
    -- Для всех записей, где еще не заполнено свойство Вид подписи, задать значения по умолчанию. 
    UPDATE Sungero_Core_Recipient
    SET SignKindDirRX_HRSolut_DirRX = 'AsDepartment'
    WHERE Discriminator = 'B7905516-2BE5-4931-961C-CB38D5677565'
          AND SignKindDirRX_HRSolut_DirRX is NULL
  end
  
-- Заполнить новые свойства в справочнике Подразделения. 
if exists (SELECT * 
           FROM information_schema.COLUMNS 
           WHERE table_name = 'Sungero_Core_Recipient' 
                 AND column_name = 'SignKindDirRX1_HRSolut_DirRX')

  begin
    -- Для всех записей, где еще не заполнено свойство Вид подписи, задать значения по умолчанию. 
    UPDATE Sungero_Core_Recipient
    SET SignKindDirRX1_HRSolut_DirRX = 'AsBusinessUnit'
    WHERE Discriminator = '61B1C19F-26E2-49A5-B3D3-0D3618151E12' 
          AND SignKindDirRX1_HRSolut_DirRX is NULL
  end