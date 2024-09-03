import React, { useEffect, useContext, memo } from 'react';
import { createUseStyles } from 'react-jss';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { StatusContext } from '../../App';
import { apiDirectDecorator, apiDecoratorOptions } from '../../helpers/api';
import { verifyEmailVerificationCode } from '../../services/autheticationService';

var styles = createUseStyles({
    wrapper: {
        textAlign: "center"
    }
})

const EmailVerification = ({emailCode}) =>
{
    const classes = styles();
    const navigate = useNavigate();
    const { t } = useTranslation(null, { keyPrefix: "login"} );
    const { setLoading, setServerErrorMessage, serverErrorMessage } = useContext(StatusContext);

    useEffect(() => {
        apiDirectDecorator(
            async () => await verifyEmailVerificationCode(emailCode),
            apiDecoratorOptions(
                { setLoading, setServerErrorMessage },
                () => { setTimeout(() => { navigate('/')}, 2000) },
                () => {}))
            ();
    },[emailCode, navigate, setLoading, setServerErrorMessage]);

    return (
        <div className={classes.wrapper} data-test="email-verification">
            {!serverErrorMessage &&
                <div data-test="email-verification-message">
                    {t("email-confirmed")}
                </div>
            }
        </div>
    )
}

export default memo(EmailVerification);