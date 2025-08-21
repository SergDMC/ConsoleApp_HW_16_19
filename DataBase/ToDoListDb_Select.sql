-- Получить все списки пользователя
SELECT * FROM "ToDoList"
WHERE "UserId" = @UserId;

-- Получить все задачи пользователя
SELECT * FROM "ToDoItem"
WHERE "UserId" = @UserId;

-- Получить все задачи по списку
SELECT * FROM "ToDoItem"
WHERE "ListId" = @ListId;

-- Получить задачу по Id
SELECT * FROM "ToDoItem"
WHERE "Id" = @ItemId;

-- Получить пользователя по TelegramUserId
SELECT * FROM "ToDoUser"
WHERE "TelegramUserId" = @TelegramUserId;

-- Получить все списки вместе с задачами (JOIN)
SELECT l."Id" AS "ListId", l."Title" AS "ListTitle",
       i."Id" AS "ItemId", i."Title" AS "ItemTitle", i."State"
FROM "ToDoList" l
LEFT JOIN "ToDoItem" i ON l."Id" = i."ListId"
WHERE l."UserId" = @UserId;
