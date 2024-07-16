import React, { useEffect, useContext } from 'react';
import { createUseStyles } from 'react-jss';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { StatusContext } from '../../App';
import { verifyEmailVerificationCode } from '../../services/autheticationService';
import { apiDirectDecorator, apiDecoratorOptions } from '../../helpers/api';

var styles = createUseStyles({
    wrapper: {
        textAlign: "center"
    }
})

export default function EmailVerification({emailCode})
{
    const classes = styles();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const statusContext = useContext(StatusContext);

    useEffect(() => {
        apiDirectDecorator(
            async () => await verifyEmailVerificationCode(emailCode),
            apiDecoratorOptions(
                statusContext,
                (data) => { setTimeout(() => { navigate('/')}, 2000) },
                () => {}))
            ();
    },[emailCode, navigate]);

    return (
        <div className={classes.wrapper} data-test="email-verification">
            {!statusContext.serverErrorMessage &&
                <div data-test="email-verification-message">
                    {t('login.email-confirmed')}
                </div>
            }
        </div>
    )
}