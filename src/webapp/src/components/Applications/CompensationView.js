import React, { memo } from 'react';
import { useTranslation } from 'react-i18next';
import { getAccessTokenClaimValue } from '../../helpers/accessTokensStorage';

const culture = getAccessTokenClaimValue('culture');
const currencySymbol = (0).toLocaleString(culture, { style: 'currency', currency: 'USD', minimumFractionDigits: 0, maximumFractionDigits: 0 }).replace(/\d/g, '').trim()

const CompensationView = ({min, max, type}) =>
{
    const { t } = useTranslation();

    const onlyMin = !!min && !!!max;
    const onlyMax = !!max && !!!min;
    const display = !!(min || max);

    const displayCompensationType = (type) =>
    {
        switch (type){
            case 'OneTime':
                return t("application.compensation-type-one-time");
            case 'Salary':
                return t("application.compensation-type-salary");
            case 'Hourly':
                return t("application.compensation-type-hourly");
            default:
                return type;
        }
    }

    const displayCompensation = () => {
        if (onlyMin)
            return '>= ' + currencySymbol + min.toLocaleString();
        else if (onlyMax)
            return '<= ' + currencySymbol + max.toLocaleString();
        else
            return currencySymbol + min.toLocaleString() + ' - ' + currencySymbol + max.toLocaleString();
    }

    if (display)
        return (
            <span>
                {displayCompensation()}&nbsp;{displayCompensationType(type)}
            </span>
        )
    else
        return (<React.Fragment />)
}

export default memo(CompensationView);