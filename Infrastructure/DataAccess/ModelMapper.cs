using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ToDoListConsoleBot.Core.DataAccess;
using ToDoListConsoleBot.Core.Entities;

namespace ToDoListConsoleBot.Infrastructure.DataAccess
{
    internal static class ModelMapper
    {
        // === User ===
        public static ToDoUser MapFromModel(ToDoUserModel model)
        {
            return new ToDoUser
            {
                Id = model.Id,
                TelegramUserId = model.TelegramUserId,
                UserName = model.UserName,
                CreatedAt = model.CreatedAt
            };
        }

        public static ToDoUserModel MapToModel(ToDoUser entity)
        {
            return new ToDoUserModel
            {
                Id = entity.Id,
                TelegramUserId = entity.TelegramUserId,
                UserName = entity.UserName,
                CreatedAt = entity.CreatedAt
            };
        }

        // === Item ===
        public static ToDoItem MapFromModel(ToDoItemModel model)
        {
            return new ToDoItem
            {
                Id = model.Id,
                UserId = model.UserId,
                ListId = model.ListId,
                Title = model.Title,
                Description = model.Description,
                State = model.State,
                Deadline = model.Deadline,
                CreatedAt = model.CreatedAt
            };
        }

        public static ToDoItemModel MapToModel(ToDoItem entity)
        {
            return new ToDoItemModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ListId = entity.ListId,
                Title = entity.Title,
                Description = entity.Description,
                State = entity.State,
                Deadline = entity.Deadline,
                CreatedAt = entity.CreatedAt
            };
        }

        // === List ===
        public static ToDoList MapFromModel(ToDoListModel model)
        {
            return new ToDoList
            {
                Id = model.Id,
                UserId = model.UserId,
                Title = model.Title,
                CreatedAt = model.CreatedAt
            };
        }

        public static ToDoListModel MapToModel(ToDoList entity)
        {
            return new ToDoListModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Title = entity.Title,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}

