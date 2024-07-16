import React, { useState, useContext, useRef } from 'react';
import { createUseStyles } from 'react-jss';
import { useNavigate } from 'react-router-dom';
import ErrorPanel from '../../common/ErrorPanel';
import { UserContext } from '../../App';
import { StatusContext } from '../../App';
import { useTranslation } from 'react-i18next';
import EmailVerification from './EmailVerification';
import { useSearchParams } from "react-router-dom";
import ReCAPTCHA from 'react-google-recaptcha';
import { setBrowserNotificationsEnabledFlag } from '../../helpers/users';
import { login, processInvitation } from '../../services/autheticationService';
import { setAccessToken, getUserFromAccessToken } from '../../helpers/accessTokensStorage';

const styles = createUseStyles({
    wrapper: {
        margin: "10px",
    },
    registrationMessage: {
        marginTop: "6px",
        marginBottom: "18px",
        fontFamily: "sans-serif",
        fontSize: "smaller",
        textAlign: "center"
    },
    registrationMessageIcon: {
        color: "white",
        fontSize: "x-large",
        padding: "4px"
    },
    captchaBox: {
        width: "300px",
        margin: "auto",
        padding: "15px"
    }
})

const capchaKey = process.env.REACT_APP_CAPTCHA_SITE_KEY;

export default function LoginPanel()
{
    const classes = styles();
    const recaptcha = useRef();
    const { t } = useTranslation();
    const [searchParams] = useSearchParams();
    const emailCode = searchParams.get("code");

    const navigate = useNavigate();
    const [loginErrorMessage, setLoginErrorMessage] = useState();
    const [invitationCodeSubmitted, setInvitationCodeSubmitted] = useState();
    const [form, setFormData] = useState({email: '', password: '', invitationCode: '', isExistingMember: false});

    const { setCurrentUser } = useContext(UserContext);
    const { setServerErrorMessage } = useContext(StatusContext);

    const handleChange = (event) => {
        const val = event.target.type === "checkbox" ? event.target.checked : event.target.value;
        setFormData((prevFormData) => ({ ...prevFormData, [event.target.name]: val }));
    };

    const resetCaptcha = () => {
        recaptcha.current.reset();
    }

    const handleError = (err, check401=false) => {
        if (!err.response)
            setServerErrorMessage(err.code);
        else
        {
            if (check401 && err.response.status === 401) {
                setLoginErrorMessage(t('login.invalid-credentials'));
                return;
            }

            if (err.response?.data?.type === "CAPTCHA_FAILED") {
                resetCaptcha();
                setLoginErrorMessage(err.response.data.detail);
            }
            else
                setLoginErrorMessage(err.message);
        }
    }

    const onFormSubmit = (e) => {
        e.preventDefault();
        setLoginErrorMessage();
        setServerErrorMessage();

        let captchaValue = '';
        if (capchaKey)
        {
            captchaValue = recaptcha.current.getValue();
            if (!captchaValue) {
                alert('Please verify the reCAPTCHA!')
                return false;
            }
        }

        if (form.isExistingMember) {
            login(form.email, form.password, captchaValue)
            .then((res) => {
                setAccessToken(res.data.accessToken);
                const user = getUserFromAccessToken();
                setCurrentUser(user);
                setBrowserNotificationsEnabledFlag(user.browserNotificationsEnabled);
                navigate('/open');
            })
            .catch((err) => {
                setCurrentUser();
                handleError(err, true);
            });
        } else {
            processInvitation(form.invitationCode, form.email, form.password)
            .then(() => {
                setInvitationCodeSubmitted(true);
                setLoginErrorMessage();
            })
            .catch((err) => {
                handleError(err);
            });
        }
    }

    return (
        <div className={classes.wrapper}>
            {emailCode && <EmailVerification emailCode={emailCode} />}
            {!emailCode &&
                <fieldset>
                    {!invitationCodeSubmitted &&
                        <React.Fragment>
                            <div className={classes.registrationMessage}>
                                <span className={classes.registrationMessageIcon}>&#10034;</span>
                                {t('login.registration-closed')}
                            </div>
                            <form onSubmit={onFormSubmit}>
                                <label className="field-container">
                                    <div className="field-label">{t('login.already-member')}</div>
                                    <input type="checkbox" value={form.isExistingMember} name="isExistingMember"
                                        className="field-control" onChange={handleChange} autoComplete='false'
                                        data-test="login-already-member" />
                                </label>
                                {!form.isExistingMember &&
                                    <label className="field-container">
                                        <div className="field-label">{t('login.invitation-code')}:</div>
                                        <input type="text" size="26" value={form.invitationCode || ''} name="invitationCode"
                                            className="field-control" onChange={handleChange} required={true}
                                            autoComplete='false' data-test="login-invitation-code" />
                                    </label>
                                }
                                <label className="field-container">
                                    <div className="field-label">{t('login.email')}:</div>
                                    <input type="text" required={true} size="26" value={form.email || ''} name="email"
                                        className="field-control" onChange={handleChange} autoComplete='false'
                                        data-test="login-email" />
                                </label>
                                <label className="field-container">
                                    <div className="field-label">{t('login.password')}:</div>
                                    <input type="password" required={true} size="26" value={form.password || ''} name="password"
                                        className="field-control" onChange={handleChange} autoComplete='false'
                                        data-test="login-password" />
                                </label>
                                <div className="editor-buttons-container">
                                    {!form.isExistingMember ?
                                        <button className='editor-button' type="submit" data-test="login-join">{t('login.join')}</button>
                                        : <button className='editor-button' type="submit" data-test="login-login">{t('login.login')}</button>
                                    }
                                    {capchaKey && <ReCAPTCHA ref={recaptcha} className={classes.captchaBox} sitekey={capchaKey} />}
                                </div>
                            </form>
                        </React.Fragment>
                    }
                    {invitationCodeSubmitted && <div>{t('login.email-confirmation-sent')}</div>}
                    {loginErrorMessage && <ErrorPanel msg={loginErrorMessage} dontDisplayHeader={true} />}
                </fieldset>
            }
        </div>
    )
}