
-- Таблица пользователей
CREATE TABLE "ToDoUser" (
    "Id" SERIAL PRIMARY KEY,
    "TelegramUserId" BIGINT NOT NULL,
    "UserName" VARCHAR(100) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Таблица списков задач

CREATE TABLE "ToDoList" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INT NOT NULL,
    "Title" VARCHAR(200) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_ToDoList_User" FOREIGN KEY ("UserId")
        REFERENCES "ToDoUser" ("Id") ON DELETE CASCADE
);


-- Таблица задач

CREATE TABLE "ToDoItem" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INT NOT NULL,
    "ListId" INT NOT NULL,
    "Title" VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "State" INT NOT NULL DEFAULT 0,  -- 0 = New, 1 = InProgress, 2 = Done
    "Deadline" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_ToDoItem_User" FOREIGN KEY ("UserId")
        REFERENCES "ToDoUser" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ToDoItem_List" FOREIGN KEY ("ListId")
        REFERENCES "ToDoList" ("Id") ON DELETE CASCADE
);

-- Индексы

-- Уникальный индекс для TelegramUserId
CREATE UNIQUE INDEX "IX_ToDoUser_TelegramUserId" ON "ToDoUser" ("TelegramUserId");

-- Индексы по внешним ключам
CREATE INDEX "IX_ToDoList_UserId" ON "ToDoList" ("UserId");
CREATE INDEX "IX_ToDoItem_UserId" ON "ToDoItem" ("UserId");
CREATE INDEX "IX_ToDoItem_ListId" ON "ToDoItem" ("ListId");
