import React, { useContext } from 'react';
import { createUseStyles } from 'react-jss';
import LoginPanel from './LoginPanel';
import { useNavigate } from 'react-router-dom';
import { UserContext } from '../../App';
import Logo from '../../common/Logo';
import LogoText from '../../common/LogoText';

const styles = createUseStyles({
    wrapper: {
        height: "100%",
        width: "100%"
    },
    loginWrapper: {
        maxWidth: "600px",
        margin: "0 auto",
    },
    logo: {
        fontSize: "1.5em",
        textAlign: "center",
        fontFamily: "monospace",
    },
    logoSubtext: {
        fontSize: ".85em"
    }
})

export default function Login()
{
    const classes = styles();
    const navigate = useNavigate();
    const { currentUser } = useContext(UserContext);

    if (currentUser)
        navigate("/open")
    else
        return (
            <div className={classes.wrapper}>
                <div className={classes.logo}>
                    <Logo />
                    <LogoText />
                </div>
                <div className={classes.loginWrapper}>
                    <LoginPanel />
                </div>
            </div>
        )
}