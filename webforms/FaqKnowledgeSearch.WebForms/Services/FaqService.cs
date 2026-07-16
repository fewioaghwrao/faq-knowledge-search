using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FaqKnowledgeSearch.WebForms.Data;
using FaqKnowledgeSearch.WebForms.Dtos;
using FaqKnowledgeSearch.WebForms.Models;

namespace FaqKnowledgeSearch.WebForms.Services
{
    public sealed class FaqService : IFaqService
    {
        public IReadOnlyList<FaqListItemDto> SearchPublishedFaqs(
            string keyword)
        {
            using (var db = new FaqKnowledgeDbContext())
            {
                var query = db.Faqs
                    .AsNoTracking()
                    .Include(x => x.Category)
                    .Include(x => x.Tags)
                    .Where(x =>
                        x.IsPublished &&
                        !x.IsDeleted &&
                        x.Category.IsActive);

                keyword =
                    (keyword ?? string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(x =>
                        x.Question.Contains(keyword) ||
                        x.Answer.Contains(keyword) ||
                        x.Category.Name.Contains(keyword) ||
                        x.Tags.Any(tag =>
                            tag.Name.Contains(keyword)));
                }

                var faqs = query
                    .OrderByDescending(x => x.UpdatedAt)
                    .ThenByDescending(x => x.Id)
                    .ToList();

                return faqs
                    .Select(x => new FaqListItemDto
                    {
                        Id = x.Id,
                        CategoryName = x.Category.Name,
                        Question = x.Question,
                        Answer = x.Answer,
                        IsPublished = x.IsPublished,
                        IsDeleted = x.IsDeleted,
                        ViewCount = x.ViewCount,
                        UpdatedAt = x.UpdatedAt,

                        TagNames = string.Join(
                            ",",
                            x.Tags
                                .OrderBy(tag => tag.DisplayOrder)
                                .ThenBy(tag => tag.Id)
                                .Select(tag => tag.Name))
                    })
                    .ToList();
            }
        }

        public FaqDetailDto GetPublishedFaqById(
            long id)
        {
            if (id <= 0)
            {
                return null;
            }

            using (var db = new FaqKnowledgeDbContext())
            {
                var faq = db.Faqs
                    .Include(x => x.Category)
                    .Include(x => x.Tags)
                    .SingleOrDefault(x =>
                        x.Id == id &&
                        x.IsPublished &&
                        !x.IsDeleted &&
                        x.Category.IsActive);

                if (faq == null)
                {
                    return null;
                }

                faq.ViewCount += 1;

                db.SaveChanges();

                return new FaqDetailDto
                {
                    Id = faq.Id,
                    CategoryName = faq.Category.Name,
                    Question = faq.Question,
                    Answer = faq.Answer,
                    ViewCount = faq.ViewCount,
                    UpdatedAt = faq.UpdatedAt,

                    TagNames = string.Join(
                        ",",
                        faq.Tags
                            .OrderBy(tag => tag.DisplayOrder)
                            .ThenBy(tag => tag.Id)
                            .Select(tag => tag.Name))
                };
            }
        }

        public IReadOnlyList<FaqListItemDto>
            SearchFaqsForAdmin(
                string keyword,
                bool? isPublished)
        {
            using (var db = new FaqKnowledgeDbContext())
            {
                var query = db.Faqs
                    .AsNoTracking()
                    .Include(x => x.Category)
                    .Where(x => !x.IsDeleted);

                keyword =
                    (keyword ?? string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(x =>
                        x.Question.Contains(keyword) ||
                        x.Answer.Contains(keyword) ||
                        x.Category.Name.Contains(keyword) ||
                        x.Tags.Any(tag =>
                            tag.Name.Contains(keyword)));
                }

                if (isPublished.HasValue)
                {
                    query = query.Where(x =>
                        x.IsPublished ==
                        isPublished.Value);
                }

                return query
                    .OrderByDescending(x => x.UpdatedAt)
                    .ThenByDescending(x => x.Id)
                    .Select(x => new FaqListItemDto
                    {
                        Id = x.Id,
                        CategoryName = x.Category.Name,
                        Question = x.Question,
                        Answer = x.Answer,
                        IsPublished = x.IsPublished,
                        IsDeleted = x.IsDeleted,
                        ViewCount = x.ViewCount,
                        UpdatedAt = x.UpdatedAt
                    })
                    .ToList();
            }
        }

        public IReadOnlyList<CategoryOptionDto>
            GetCategoryOptions(
                long? includeCategoryId)
        {
            using (var db = new FaqKnowledgeDbContext())
            {
                return db.Categories
                    .AsNoTracking()
                    .Where(x =>
                        x.IsActive ||
                        (includeCategoryId.HasValue &&
                         x.Id ==
                         includeCategoryId.Value))
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Id)
                    .Select(x =>
                        new CategoryOptionDto
                        {
                            Id = x.Id,
                            Name = x.Name,
                            IsActive = x.IsActive
                        })
                    .ToList();
            }
        }

        public IReadOnlyList<TagOptionDto>
            GetTagOptions()
        {
            using (var db = new FaqKnowledgeDbContext())
            {
                return db.Tags
                    .AsNoTracking()
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Id)
                    .Select(x =>
                        new TagOptionDto
                        {
                            Id = x.Id,
                            Name = x.Name,
                            DisplayOrder =
                                x.DisplayOrder
                        })
                    .ToList();
            }
        }

        public FaqEditDto GetFaqForAdmin(
            long id)
        {
            if (id <= 0)
            {
                return null;
            }

            using (var db = new FaqKnowledgeDbContext())
            {
                var faq = db.Faqs
                    .AsNoTracking()
                    .Include(x => x.Tags)
                    .SingleOrDefault(x =>
                        x.Id == id &&
                        !x.IsDeleted);

                if (faq == null)
                {
                    return null;
                }

                return new FaqEditDto
                {
                    Id = faq.Id,
                    CategoryId = faq.CategoryId,
                    Question = faq.Question,
                    Answer = faq.Answer,
                    IsPublished = faq.IsPublished,

                    SelectedTagIds = faq.Tags
                        .OrderBy(x => x.DisplayOrder)
                        .ThenBy(x => x.Id)
                        .Select(x => x.Id)
                        .ToList()
                };
            }
        }

        public long CreateFaq(
            FaqEditDto input)
        {
            ValidateFaqInput(input);

            using (var db = new FaqKnowledgeDbContext())
            {
                var categoryExists =
                    db.Categories.Any(x =>
                        x.Id == input.CategoryId &&
                        x.IsActive);

                if (!categoryExists)
                {
                    throw new InvalidOperationException(
                        "選択されたカテゴリは使用できません。");
                }

                var selectedTags =
                    GetSelectedTags(
                        db,
                        input.SelectedTagIds);

                var now = DateTime.Now;

                var faq = new Faq
                {
                    CategoryId = input.CategoryId,
                    Question = input.Question,
                    Answer = input.Answer,
                    IsPublished = input.IsPublished,
                    IsDeleted = false,
                    ViewCount = 0,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                foreach (var tag in selectedTags)
                {
                    faq.Tags.Add(tag);
                }

                db.Faqs.Add(faq);
                db.SaveChanges();

                return faq.Id;
            }
        }

        public bool UpdateFaq(
            long id,
            FaqEditDto input)
        {
            if (id <= 0)
            {
                return false;
            }

            ValidateFaqInput(input);

            using (var db = new FaqKnowledgeDbContext())
            {
                var faq = db.Faqs
                    .Include(x => x.Tags)
                    .SingleOrDefault(x =>
                        x.Id == id &&
                        !x.IsDeleted);

                if (faq == null)
                {
                    return false;
                }

                /*
                 * 現在設定されているカテゴリが無効の場合は、
                 * 同じカテゴリを維持する編集だけ許可する。
                 */
                var categoryExists =
                    db.Categories.Any(x =>
                        x.Id == input.CategoryId &&
                        (x.IsActive ||
                         x.Id == faq.CategoryId));

                if (!categoryExists)
                {
                    throw new InvalidOperationException(
                        "選択されたカテゴリは使用できません。");
                }

                var selectedTags =
                    GetSelectedTags(
                        db,
                        input.SelectedTagIds);

                faq.CategoryId = input.CategoryId;
                faq.Question = input.Question;
                faq.Answer = input.Answer;
                faq.IsPublished = input.IsPublished;
                faq.UpdatedAt = DateTime.Now;

                /*
                 * いったん中間テーブルの関連を解除し、
                 * 画面で選択されたタグを設定し直す。
                 */
                faq.Tags.Clear();

                foreach (var tag in selectedTags)
                {
                    faq.Tags.Add(tag);
                }

                db.SaveChanges();

                return true;
            }
        }

        private static void ValidateFaqInput(
            FaqEditDto input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(
                    "input");
            }

            var question =
                (input.Question ??
                 string.Empty).Trim();

            var answer =
                (input.Answer ??
                 string.Empty).Trim();

            if (input.CategoryId <= 0)
            {
                throw new ArgumentException(
                    "カテゴリを選択してください。");
            }

            if (string.IsNullOrWhiteSpace(
                question))
            {
                throw new ArgumentException(
                    "質問を入力してください。");
            }

            if (question.Length > 500)
            {
                throw new ArgumentException(
                    "質問は500文字以内で入力してください。");
            }

            if (string.IsNullOrWhiteSpace(
                answer))
            {
                throw new ArgumentException(
                    "回答を入力してください。");
            }

            input.Question = question;
            input.Answer = answer;

            input.SelectedTagIds =
                (input.SelectedTagIds ??
                 new List<int>())
                .Where(x => x > 0)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 選択されたタグをDBから取得し、
        /// 存在しないタグIDが含まれていないか確認します。
        /// </summary>
        private static IList<Tag> GetSelectedTags(
            FaqKnowledgeDbContext db,
            IEnumerable<int> selectedTagIds)
        {
            var tagIds =
                (selectedTagIds ??
                 Enumerable.Empty<int>())
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (tagIds.Count == 0)
            {
                return new List<Tag>();
            }

            var tags = db.Tags
                .Where(x =>
                    tagIds.Contains(x.Id))
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToList();

            if (tags.Count != tagIds.Count)
            {
                throw new InvalidOperationException(
                    "選択されたタグの一部が存在しません。");
            }

            return tags;
        }

        public bool DeleteFaq(
            long id)
        {
            if (id <= 0)
            {
                return false;
            }

            using (var db = new FaqKnowledgeDbContext())
            {
                var faq = db.Faqs
                    .SingleOrDefault(x =>
                        x.Id == id &&
                        !x.IsDeleted);

                if (faq == null)
                {
                    return false;
                }

                faq.IsDeleted = true;
                faq.IsPublished = false;
                faq.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                return true;
            }
        }
    }
}