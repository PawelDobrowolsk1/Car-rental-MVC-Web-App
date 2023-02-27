﻿using AutoMapper;
using Car_Rental_MVC.Entities;
using Car_Rental_MVC.Exceptions;
using Car_Rental_MVC.Models;
using Car_Rental_MVC.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Car_Rental_MVC.Repositories
{
    public class CarRepository2 : Repository<Car, CarModelDto>, ICarRepository2
    {
        private readonly CarRentalManagerContext _context;
        private readonly IMapper _mapper;

        public CarRepository2(CarRentalManagerContext context, IMapper mapper) : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddCarAsync(CarModelDto carDto)
        {
            var car = _mapper.Map<Car>(carDto);
            await AddAsync(car);
        }

        public async Task Update(Car car)
        {
            _context.Cars.Update(car);
            await Task.CompletedTask;
        }

        public async Task DeleteCarAsync(int carId)
        {
            var car = _context.Cars.FirstOrDefault(x => x.Id == carId);
            if (car == null)
            {
                throw new NotFoundException("Car not found");
            }
            await DeleteAsync(car);
        }

        public async Task RentCarAsync(string email, int carId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email) ?? throw new NotFoundException("User not found");
            var car = _context.Cars.FirstOrDefault(c => c.Id == carId) ?? throw new NotFoundException("Car not found");

            car.Available = false;

            var rentInfo = new RentCarInfo()
            {
                UserId = user.Id,
                CarId = car.Id,
                IsGivenBack = false
            };

            _context.RentInfo.Add(rentInfo);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<CarModelDto>> RentedCarsByUser(string email)
        {
            var user = _context
                .Users
                .FirstOrDefault(u => u.Email == email) ?? throw new NotFoundException("User not found");

            var rentedCarInfo = _context
                .RentInfo
                .Include(c => c.Car)
                .Where(u => u.UserId == user.Id && u.IsGivenBack == false)
                .ToList();

            var carsDtosList = new List<CarModelDto>();
            if (rentedCarInfo.Any())
            {
                foreach (var car in rentedCarInfo)
                {
                    carsDtosList.Add(_mapper.Map<CarModelDto>(car.Car));
                }
                return await Task.FromResult(carsDtosList);
            }
            return null;
        }

        public async Task ReturnCar(string email, int carId)
        {
            var user = _context
                .Users
                .SingleOrDefault(u => u.Email == email) ?? throw new NotFoundException("User not found");

            var car = _context
                .Cars
                .SingleOrDefault(c => c.Id == carId) ?? throw new NotFoundException("Car not found");

                car.Available = true;

            var rentedInfo = _context
                .RentInfo
                .SingleOrDefault(x => x.UserId == user.Id && x.CarId == car.Id && x.IsGivenBack == false);
            if (rentedInfo != null)
            {
                rentedInfo.IsGivenBack = true;
            }
            await Task.CompletedTask;
        }
    }
}