﻿using AutoMapper;
using Discount.Grpc.Entities;
using Discount.Grpc.Protos;
using Discount.Grpc.Repositories;
using Grpc.Core;

namespace Discount.Grpc.Services
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DiscountService> _logger;

        public DiscountService(IDiscountRepository discountRepository, IMapper mapper, ILogger<DiscountService> logger)
        {
            _discountRepository = discountRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var coupon = await _discountRepository.GetDiscount(request.ProductName);

            if (coupon == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Discount with ProductName={request.ProductName} is not found."));
            }

            _logger.LogInformation($"Discount is retrieved for ProductName : {coupon.ProductName}, Amount {coupon.Amount}");

            var couponModel = _mapper.Map<CouponModel>(coupon);
            return couponModel;
        }

        public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
        {
            var coupon = _mapper.Map<Coupon>(request.Coupon);

            var isCreated = await _discountRepository.CreateDiscount(coupon);

            if (!isCreated)
            {
                string message = $"Failure of creating thr discount with ProductName={request.Coupon.ProductName}, Amount={request.Coupon.Amount}.";
                _logger.LogError(message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, message));
            }

            _logger.LogInformation($"Discount is successfuly created for product {coupon.ProductName}");

            var couponModel = _mapper.Map<CouponModel>(coupon);
            return couponModel;
        }

        public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
        {
            var coupon = _mapper.Map<Coupon>(request.Coupon);

            var isUpdated = await _discountRepository.UpdateDiscount(coupon);

            if (!isUpdated)
            {
                string message = $"Failure of updating thr discount with ProductName={request.Coupon.ProductName}, Amount={request.Coupon.Amount}.";
                _logger.LogError(message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, message));
            }

            _logger.LogInformation($"Discount is successfuly updated for product {coupon.ProductName}");

            var couponModel = _mapper.Map<CouponModel>(coupon);
            return couponModel;
        }

        public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
        {
            var isDeleted = await _discountRepository.DeleteDiscount(request.ProductName);

            var response = new DeleteDiscountResponse() { Success = isDeleted };

            return response;
        }
    }
}
