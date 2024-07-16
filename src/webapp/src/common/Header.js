import React, { memo } from 'react';
import { createUseStyles } from 'react-jss'
import ExpanderMenu from './ExpanderMenu';
import Logo from './Logo';
import Greeter from './Greeter';

const styles = createUseStyles({
    wrapper: {
        display: "flex",
        flexWrap: "nowrap",
        flexDirection: "row",
        alignItems: "flex-start",
        justifyContent: "space-between"
    },
    banner: {
        fontSize: ".5em",
        marginLeft: "10px",
        marginRight: "10px"
    }
})

const Header = () =>
{
    const classes = styles();

    return (
        <div className={classes.wrapper}>
            <div className={classes.banner}>
                <Logo />
            </div>
            <div>
                <Greeter />
                <ExpanderMenu/>
            </div>
        </div>
    )
}

export default memo(Header);