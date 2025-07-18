import { useContext, memo } from 'react';
import { useTranslation } from 'react-i18next';
import { UserContext } from '../../App';

const displayCompensationType = (type, t) =>
{
    switch (type){
        case 'OneTime':
            return t("compensation-type-one-time");
        case 'Salary':
            return t("compensation-type-salary");
        case 'Hourly':
            return t("compensation-type-hourly");
        default:
            return type;
    }
}

const displayCompensation = (currencySymbol, onlyMin, onlyMax, min, max) => {
    if (onlyMin)
        return ">= " + currencySymbol + min.toLocaleString();
    else if (onlyMax)
        return "<= " + currencySymbol + max.toLocaleString();
    else
        return currencySymbol + min.toLocaleString() + " - " + currencySymbol + max.toLocaleString();
}

const CompensationView = ({min, max, type, dataTestPrefix=""}) =>
{
    const { currentUser } = useContext(UserContext);
    const currencySymbol = (0).toLocaleString(currentUser.culture,
        { style: 'currency', currency: 'USD', minimumFractionDigits: 0, maximumFractionDigits: 0 }).replace(/\d/g, '').trim()

    const { t } = useTranslation(null, { keyPrefix: "application" });

    const onlyMin = !!min && !!!max;
    const onlyMax = !!max && !!!min;
    const display = !!(min || max);

    if (display)
        return (
            <span >
                <span data-test={`${dataTestPrefix}-compensation`}>{displayCompensation(currencySymbol, onlyMin, onlyMax, min, max)}</span>
                &nbsp;
                <span data-test={`${dataTestPrefix}-compensation-type`}>{displayCompensationType(type, t)}</span>
            </span>
        )
    else
        return (<></>)
}

export default memo(CompensationView);