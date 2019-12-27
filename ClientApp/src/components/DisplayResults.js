import React, { useState, useEffect } from 'react';

const ARTIST = 'ARTIST';
const ALBUMS = 'ALBUMS';
const TRACKS = 'TRACKS';

const parseTracks = ((results) => {
    let toRender = results.map(result => {
        let { tracks, format, title } = result;
        tracks.map(row =>
            <tr key={row.id}>
                <td>{row.position}</td>
                <td>{row.id}</td>
                <td>{row.title}</td>
                <td>{title}</td>
                <td>{format}</td>
            </tr>)
    });
    return toRender;
})

//ideally, 2 methods, getHeaders(mode) & getBody(mode) with 4 other methods or components with render{MODE}Body calls
const DisplayResults = (props) => {
    var { ApiData, GetReleases, GetTracks } = props;
    const [results, setResults] = useState(null);
    const [mode, setMode] = useState(ARTIST);
    useEffect(() => {
        if (ApiData && ApiData.artists) {
            setMode(ARTIST);
            setResults(ApiData.artists);
        }
        if (ApiData && ApiData["releases"]) {
            setMode(ALBUMS);
            setResults(ApiData["releases"]);
        }
        if (ApiData && ApiData["media"]) {
            setMode(TRACKS);
            setResults(ApiData["media"]);
        }
    });

    if (!results) return null;

    var tbody = (results ?
        <tbody>
            {mode === ARTIST && results.map(row =>
                <tr key={row.id}>
                    <td><a className="action" onClick={() => GetReleases(row.id)}> {row.id}</a></td>
                    <td>{row.name}</td>
                    <td>{row.type}</td>
                    <td>{row.country}</td>
                    <td>{row.score}</td>
                </tr>)}
            {mode === ALBUMS && results.map(row =>
                <tr key={row.id}>
                    <td><a className="action" onClick={() => GetTracks(row.id)}> {row.id}</a></td>
                    <td>{row.title}</td>
                    <td>{row["primary-type"]}</td>
                    <td>{row["first-release-date"]}</td>
                </tr>)
            }
            {
                mode === TRACKS && parseTracks(results)
            }
        </tbody> : null);
    return <table>
        <thead>
            <tr>
                {mode !== TRACKS && <th>Position</th>}
                <th>ID</th>
                <th>Name</th>
                {mode === ARTIST && <th>Country</th>}
                {mode === ARTIST && <th>Score</th>}
                {mode === ALBUMS && <th>Release date</th>}
                {mode === TRACKS && <th>Release Title</th>}
                {mode === TRACKS && <th>Release format</th>}
            </tr>
        </thead>
        {tbody}
    </table>

}

export default DisplayResults;