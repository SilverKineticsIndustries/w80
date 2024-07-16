import React, { useContext, memo } from 'react';
import { createUseStyles } from 'react-jss';
import { StatusContext } from '../App';
import Spinner from './Spinner';
import { useTranslation } from 'react-i18next';

const styles = createUseStyles({
    error: {
        top: "0px",
        zIndex: "1000",
        position: "fixed",
        left: "calc(50% - 400px / 2)",
        background: "var(--dialog-background)",
        borderBottom: "1px solid var(--error-text)",
        borderLeft: "1px solid var(--error-text)",
        borderRight: "1px solid var(--error-text)",
        borderRadius: "5px",
        width: "400px",
        " & > div": {
            margin: "12px",
            color: "var(--error-text)",
            textAlign: "center"
        }
    }
})

const StatusPanel = () =>
{
    const classes = styles();
    const { t } = useTranslation();
    const { loading, serverErrorMessage } = useContext(StatusContext);
    const isLoading = loading > 0;
    const hasError = !!serverErrorMessage;

    const displayErrorMessage = (serverErrorMessage) => {
        switch (serverErrorMessage) {
            case "ERR_NETWORK":
                return t("common.network-error")
            case "ERR_CANCELLED":
                return t("common.cancelled-error")
            case "ERR_BAD_RESPONSE":
                return t("common.bad-response-error")
            case "ECONNABORTED":
                return ""; // TODO
            default:
                return serverErrorMessage;
        }
    }

    return (
        <React.Fragment>
            {hasError && !isLoading &&
                <div className={classes.error} data-test="status-panel">
                    <div data-test="status-panel-message">{displayErrorMessage(serverErrorMessage)}</div>
                </div>
            }
            {!hasError && isLoading && <Spinner />}
        </React.Fragment>
    )
}

export default memo(StatusPanel);