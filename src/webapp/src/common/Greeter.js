import React, { useContext, memo } from 'react';
import { createUseStyles } from 'react-jss'
import { UserContext } from '../App';
import { Tooltip } from 'react-tooltip';
import admin from '../assets/admin.png';
import { v4 as uuidv4 } from 'uuid';
import { useTranslation } from 'react-i18next';
import { getAccessTokenClaimValue } from '../helpers/accessTokensStorage';

const styles = createUseStyles({
    wrapper: {
        display: "inline-block",
        "@media (max-width: 375px)": {
            display: "none"
        }
    },
    useridentifier: {
        fontSize: "1.1em",
        marginRight: "10px",
        marginLeft: "10px"
    },
    admin: {
        verticalAlign: "middle"
    }
})

const uniqueid = uuidv4();

const Greeter = () => {

    const classes = styles();
    const { t } = useTranslation(null, { keyPrefix: "common"});
    const { currentUser } = useContext(UserContext);

    if (currentUser)
    {
        const role = getAccessTokenClaimValue('Role');
        const isAdmin = role === 'Administrator';

        return (
            <div className={classes.wrapper} data-test="greeter">
                {isAdmin &&
                    <>
                        <img data-tooltip-id={uniqueid} className={classes.admin} src={admin} height={20} width={20} alt={t("administrator")} />
                        <Tooltip id={uniqueid} delayShow="100" place="top">{t("administrator")}</Tooltip>
                    </>
                }
                <span className={classes.useridentifier} data-test="greeter-useridentifier">{currentUser.email}</span>
            </div>
        )
    }
    else
        return (<></>)
}

export default memo(Greeter);