using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using WS_CMVC_Demo.Data;
using WS_CMVC_Demo.Models;
using WS_CMVC_Demo.Services;

namespace WS_CMVC_Demo
{
    public static class Extensions
    {
        /// <summary>
        /// Remove ALL whitespace from String
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }

        /// <summary>
        /// Возвращает идентификатор пользователя
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static Guid? GetId(this System.Security.Claims.ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (idClaim != null)
                return Guid.Parse(idClaim.Value);

            return null;
        }

        #region IdentityHelper
        /// <summary>
        /// Проверяет пользователя на уникальность
        /// </summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<IDictionary<string, string>> CheckUserAsync(this ApplicationUser user, ApplicationDbContext context, Guid? ExcludeUserId = null)
        {
            var res = new Dictionary<string, string>();

            if (await context.Users.Where(u => u.Email == user.Email && u.Id != ExcludeUserId).AnyAsync())
            {
                res.Add(nameof(user.Email), "В системе уже зарегистрирован пользователь с таким Email.");
            }

            if (await context.Users.Where(u => u.PhoneNumber == user.PhoneNumber && u.Id != ExcludeUserId).AnyAsync())
            {
                res.Add(nameof(user.PhoneNumber), "В системе уже зарегистрирован пользователь с таким телефонным номером.");
            }

            if (await context.Users.Where(u => u.PassportNumber == user.PassportNumber && u.Id != ExcludeUserId).AnyAsync())
            {
                res.Add(nameof(user.PassportNumber), "В системе уже зарегистрирован пользователь с таким паспартом.");
            }

            if (await context.Users.Where(u => u.SecondName == user.SecondName && u.Name == user.Name && u.MiddleName == user.MiddleName && u.Id != ExcludeUserId).AnyAsync())
            {
                res.Add(nameof(user.SecondName), "В системе уже зарегистрирован пользователь с ФИО.");
                res.Add(nameof(user.Name), "В системе уже зарегистрирован пользователь с ФИО.");
                res.Add(nameof(user.MiddleName), "В системе уже зарегистрирован пользователь с ФИО.");
            }

            var subcat = await context.UserSubcategories.FindAsync(user.UserSubcategoryId);

            if (subcat.CategoryId != user.UserCategoryId)
            {
                res.Add(nameof(user.UserSubcategoryId), "Данная подкатегория недоступна для вашей категории.");
            }

            // Выставляем NULL в полях не для этой подкатегории
            if (subcat.ExcludeProperties != null)
            {
                foreach (var excludePropName in subcat.ExcludeProperties)
                {
                    var fi = typeof(ApplicationUser).GetProperty(excludePropName);
                    if (fi != null)
                    {
                        fi.SetValue(user, null);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Наплевательски проверяет пользователя на уникальность (Быстрая регистрация)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static async Task<IDictionary<string, string>> CheckQuickUserAsync(this ApplicationUser user, ApplicationDbContext context, Guid? ExcludeUserId = null)
        {
            var res = new Dictionary<string, string>();

            if (await context.Users.Where(u => u.PhoneNumber == user.PhoneNumber && u.Id != ExcludeUserId).AnyAsync())
            {
                res.Add(nameof(user.PhoneNumber), "В системе уже зарегистрирован пользователь с таким телефонным номером.");
            }

            if (await context.Users.Where(u => u.SecondName == user.SecondName && u.Name == user.Name && u.MiddleName == user.MiddleName && u.Id != ExcludeUserId).AnyAsync())
            {
                res.Add(nameof(user.SecondName), "В системе уже зарегистрирован пользователь с ФИО.");
                res.Add(nameof(user.Name), "В системе уже зарегистрирован пользователь с ФИО.");
                res.Add(nameof(user.MiddleName), "В системе уже зарегистрирован пользователь с ФИО.");
            }

            var subcat = await context.UserSubcategories.FindAsync(user.UserSubcategoryId);

            if (subcat.CategoryId != user.UserCategoryId)
            {
                res.Add(nameof(user.UserSubcategoryId), "Данная подкатегория недоступна для вашей категории.");
            }

            // Выставляем NULL в полях не для этой подкатегории
            if (subcat.ExcludeProperties != null)
            {
                foreach (var excludePropName in subcat.ExcludeProperties)
                {
                    var fi = typeof(ApplicationUser).GetProperty(excludePropName);
                    if (fi != null)
                    {
                        fi.SetValue(user, null);
                    }
                }
            }

            return res;
        }

        public static string ConvertPhoneNumber(string phoneNumber)
        {
            char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            var phone = phoneNumber.ToArray().Where(c => digits.Contains(c)).ToArray();
            if (phone?.Length == 0)
            {
                return null;
            }
            if (phone[0] == '8')
            {
                phone[0] = '7';
            }
            return string.Join(null, phone);
        }

        
        #endregion 

        #region ControllerExtensions
        public static void AddErrors(this Controller controller, IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                controller.ModelState.AddModelError("", error.Description);
            }
        }

        public static void AddErrors(this Controller controller, IDictionary<string, string> result)
        {
            foreach (var error in result)
            {
                controller.ModelState.AddModelError(error.Key, error.Value);
            }
        }

        public static ActionResult RedirectToLocal(this Controller controller, string returnUrl)
        {
            if (controller.Url.IsLocalUrl(returnUrl))
            {
                return controller.Redirect(returnUrl);
            }
            return controller.RedirectToAction("Index", "Home");
        }


        /// <summary>
        /// Автоматическая бронь первого доступного пакета для данной категории
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public static async Task AutoPackage(ApplicationUser user, ApplicationDbContext _context)
        {
            try
            {
                if (user != null)
                {
                    var userid = user.Id;
                    var subcatid = user.UserSubcategoryId;
                    //Проверка, есть ли Автопакет
                    if (await _context.UserSubcategories.Where(us => us.Id == subcatid).Select(res => res.AutoPackage).FirstOrDefaultAsync())
                    {
                        var eventid = 1;
                        if (subcatid != null)
                        {
                            var datenow = DateTime.Now;
                            var package = await _context.UserSubcategoryEventPackages.Where(re => re.UserSubcategoryEvent.UserSubcategoryId == subcatid && re.UserSubcategoryEvent.EventId == eventid && re.UserSubcategoryEvent.Event.DateEnd >= datenow).Select(r => r.Package).FirstOrDefaultAsync();
                            var PackageServices = await _context.PackageServices.Where(ps => ps.Package == package).Include(ps => ps.Service.ServiceType).OrderBy(ps => ps.Service.ServiceType.Id).ToListAsync();
                            foreach (var packageService in PackageServices)
                            {
                                //Проверяем, была ли уже выбрана ранее эта услуга.
                                var alreadyChoosed = await _context.UserPackageServices.Where(ups => ups.UserId == userid && ups.PackageService == packageService).FirstOrDefaultAsync();
                                if (alreadyChoosed == null)
                                {
                                    var StartDate = packageService.StartDate;
                                    var EndDate = packageService.FinishDate;

                                    var ups = new Models.Service.UserPackageService
                                    {
                                        PackageServiceId = packageService.Id,
                                        StartDate = packageService.StartDate,
                                        FinishDate = packageService.FinishDate,
                                        CreateUserId = userid,
                                        EventId = eventid,
                                        UserId = userid,
                                        Status = Models.Service.UserPackageServiceStatus.accepted
                                    };
                                    await _context.UserPackageServices.AddAsync(ups);
                                }
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }

            }
            catch { }
        }
        #endregion
    }

    public class MultilanguageIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError()
            {
                Code = nameof(DuplicateEmail),
                Description = "В системе уже зарегистрирован пользователь с таким Email."
            };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError()
            {
                Code = nameof(DuplicateUserName),
                Description = "В системе уже зарегистрирован пользователь с таким именем пользователя."
            };
        }
    }

    public static class EnumHelper<T>
        where T : struct, Enum // This constraint requires C# 7.3 or later.
    {
        public static IList<T> GetValues(Enum value)
        {
            var enumValues = new List<T>();

            foreach (FieldInfo fi in value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                enumValues.Add((T)Enum.Parse(value.GetType(), fi.Name, false));
            }
            return enumValues;
        }

        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static IList<string> GetNames(Enum value)
        {
            return value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public).Select(fi => fi.Name).ToList();
        }

        public static IList<string> GetDisplayValues(Enum value)
        {
            return GetNames(value).Select(obj => GetDisplayValue(Parse(obj))).ToList();
        }

        private static string LookupResource(Type resourceManagerProvider, string resourceKey)
        {
            var resourceKeyProperty = resourceManagerProvider.GetProperty(resourceKey,
                BindingFlags.Static | BindingFlags.Public, null, typeof(string),
                Array.Empty<Type>(), null);
            if (resourceKeyProperty != null)
            {
                return (string)resourceKeyProperty.GetMethod.Invoke(null, null);
            }

            return resourceKey; // Fallback with the key name
        }

        public static string GetDisplayValue(T value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes[0].ResourceType != null)
                return LookupResource(descriptionAttributes[0].ResourceType, descriptionAttributes[0].Name);

            if (descriptionAttributes == null) return string.Empty;
            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Name : value.ToString();
        }

    }
}
