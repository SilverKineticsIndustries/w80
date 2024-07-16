const CustomErrorTypes = {
    HasValidationErrors: 'HasValidationErrors',
    InvalidRefreshToken: 'InvalidRefreshToken'
}

export function isValidationErrors(err)
{
    return err?.response?.status === 400 && err?.response?.data?.type === CustomErrorTypes.HasValidationErrors;
}

export function isRefreshTokenInvalid(err)
{
    return err?.response?.status === 401 && err?.response?.data?.type === CustomErrorTypes.InvalidRefreshToken;
}