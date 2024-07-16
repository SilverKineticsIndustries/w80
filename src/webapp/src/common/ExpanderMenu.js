import React, { useState, useContext } from 'react';
import { createUseStyles } from 'react-jss'
import { createPortal } from 'react-dom';
import { UserContext } from '../App';
import { post } from '../helpers/api';
import { useNavigate } from 'react-router-dom';
import UserProfile from '../components/UserProfile/UserProfile';
import UserManagementList from '../components/UserManagement/UserManagementList';
import { getAccessTokenClaimValue, clearAccessToken } from '../helpers/accessTokensStorage';

const styles = createUseStyles({
    wrapper: {
        display: "inline-block",
        position: "relative",
        paddingRight: "10px"
    },
    expander: {
        display: "block",
        position: "relative",
        fontSize: "50px",
        cursor: "pointer",
        opacity: ".8",
        transition: "0.3s",
        '&:hover': {
            color: "var(--lighter)",
            opacity: 1
        },
        '&:hover + div': {
            display: "block"
        }
    },
    menu: {
        display: "none",
        top: "60px",
        right: "10px",
        position: "absolute",
        zIndex: 10000,
        backgroundColor: "var(--semi-dark)",
        '&:hover': {
            display: "block"
        }
    }
});

export default function ExpanderMenu()
{
    const classes = styles();
    const navigate = useNavigate();
    const { setCurrentUser } = useContext(UserContext);
    const [userProfileVisible, setUserProfileVisible] = useState(false);
    const [userManagementVisible, setUserManagementVisible] = useState(false);

    const onLogout = () => {
        post("/authentication/logout", {}, true, true)
        .finally(() => {
            clearAccessToken();
            setCurrentUser();
            navigate('/');
        });
    }

    const role = getAccessTokenClaimValue('Role');
    const isAdministrator = role === 'Administrator';

    return (
        <div className={classes.wrapper}>
            <div className={classes.expander}>
                &#10050;
            </div>
            <div className={classes.menu} data-test="header-expandermenu">
                <button className='header-menu-button' data-test="header-showprofile" onClick={() => setUserProfileVisible(true)}>Profile</button>
                {isAdministrator && <button className='header-menu-button' data-test="header-showusermanagement" onClick={() => setUserManagementVisible(true)}>User Management</button>}
                <button className='header-menu-button' data-test="header-logout" onClick={() => onLogout()}>Logout</button>
            </div>
            {userProfileVisible && createPortal(
                <UserProfile onClose={() => setUserProfileVisible(false)} />,
                document.body
            )}
            {userManagementVisible && createPortal(
                <UserManagementList onClose={() => setUserManagementVisible(false)} />,
                document.body
            )}
        </div>
    )
}