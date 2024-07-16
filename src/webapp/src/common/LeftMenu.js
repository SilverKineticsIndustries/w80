import React, { memo } from 'react';
import { createUseStyles } from 'react-jss';
import applications from '../assets/applications.png';
import calendar from '../assets/calendar.png';
import archive from '../assets/archive.png';
import reject from '../assets/reject.png';
import accept from '../assets/accept.png';
import stats from '../assets/stats.png';
import LeftMenuItem from './LeftMenuItem';
import { useTranslation } from 'react-i18next';

const styles = createUseStyles({
    list: {
        display: "flex",
        flexDirection: "column",
        listStyleType: "none",
        paddingTop: "0px",
        paddingLeft: "12px",
        paddingRight: "12px",
        marginTop: "10px",
        zIndex: "1000",
        "@media (max-width: 720px)": {
            flexDirection: "row"
        }
    }
})

const LeftMenu = () =>
{
    const classes = styles();
    const { t } = useTranslation();

    return (
        <ul className={classes.list} data-test="left-menu">
            <LeftMenuItem path='/open' img={applications} tooltip={t("common.view-open-applications")} dataTest="left-menu-open" />
            <LeftMenuItem path='/calendar' img={calendar} tooltip={t("common.view-calendar")} dataTest="left-menu-calendar" />
            <LeftMenuItem path='/archived' img={archive} tooltip={t("common.view-archived-applications")} dataTest="left-menu-archived" />
            <LeftMenuItem path='/accepted' img={accept} tooltip={t("common.view-accepted-applications")} dataTest="left-menu-accepted" />
            <LeftMenuItem path='/rejected' img={reject} tooltip={t("common.view-rejected-applications")} dataTest="left-menu-rejected" />
            <LeftMenuItem path='/statistics' img={stats} tooltip={t("common.view-statistics")} dataTest="left-menu-statistics" />
        </ul>
    )
}

export default memo(LeftMenu)
