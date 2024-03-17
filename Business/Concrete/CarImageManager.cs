using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants;
using Castle.Core.Internal;
using Core.Utilities.Results;
using Core.Utilities.Business;
using Core.Utilities.Helpers.FileHelper;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;

namespace Business.Concrete
{
    public class CarImageManager : ICarImageService
    {
        private ICarImageDal _carImageDal;
        private IFileHelper _fileHelper;

        public CarImageManager(ICarImageDal carImageDal, IFileHelper fileHelper)
        {
            _carImageDal = carImageDal;
            _fileHelper = fileHelper;
        }

        [SecuredOperation("carImage.add,admin")]
        public IResult Add(IFormFile file, CarImage carImage)
        {
            IResult result = BusinessRules.Run(CheckCarImageLimitByCarId(carImage.CarId));
            if (result != null)
            {
                return result;
            }

            carImage.ImagePath = _fileHelper.Upload(file, ImagePats.Pats);
            carImage.Date = DateTime.Now;

            _carImageDal.Add(carImage);
            return new SuccessResult(Messages.AddedImage);
        }

        public IDataResult<List<CarImage>> GetAll()
        {
            return new SuccessDataResult<List<CarImage>>(_carImageDal.GetAll(), Messages.ListedImages);
        }

        public IDataResult<CarImage> GetById(int id)
        {
            return new SuccessDataResult<CarImage>(_carImageDal.Get(img => img.Id == id));
        }

        public IDataResult<List<CarImage>> GetImagesByCarId(int carId)
        {
            var result = BusinessRules.Run(CheckCarImage(carId));
            if (result != null)
            {
                return new ErrorDataResult<List<CarImage>>(GetDefaultCarImage(carId).Data);
            }
            return new SuccessDataResult<List<CarImage>>(_carImageDal.GetAll(img => img.CarId == carId),
                Messages.ListedImages);
        }

        [SecuredOperation("carImage.update,admin")]
        public IResult Update(IFormFile file, CarImage carImage)
        {
            carImage.ImagePath = _fileHelper.Update(file, ImagePats.Pats + carImage.ImagePath, ImagePats.Pats);
            carImage.Date = DateTime.Now;
            _carImageDal.Update(carImage);
            return new SuccessResult(Messages.UptadedCarImage);
        }

        [SecuredOperation("carImage.delete,admin")]
        public IResult Delete(CarImage carImage)
        {
            _fileHelper.Delete(ImagePats.Pats + carImage.ImagePath);
            _carImageDal.Delete(carImage);
            return new SuccessResult(Messages.DeletedCarImage);
        }

        private IResult CheckCarImageLimitByCarId(int carId)
        {
            var result = _carImageDal.GetAll(img => img.CarId == carId);
            if (result.Count >= 5)
            {
                return new ErrorResult(Messages.LimitExedded);
            }
            return new SuccessResult();
        }

        private IResult CheckCarImage(int carId)
        {
            var result = _carImageDal.GetAll(img => img.CarId == carId);
            if (result.Count > 0)
            {
                return new SuccessResult();
            }

            return new ErrorResult();
        }

        private IDataResult<List<CarImage>> GetDefaultCarImage(int carId)
        {
            List<CarImage> carImage = new List<CarImage>() { new CarImage() { CarId = carId, Date = DateTime.Now, ImagePath = @"wwwroot\Uploads\Images\logo.jpg" } };
            return new SuccessDataResult<List<CarImage>>(carImage);
        }
    }
}
