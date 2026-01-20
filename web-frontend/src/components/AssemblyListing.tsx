import React from 'react';
import { ListingLine } from '../types/api';
import './AssemblyListing.css';

interface AssemblyListingProps {
  listingLines: ListingLine[];
  currentAddress?: number; // IAR for highlighting
  executionSpeed: number; // Instructions per second
}

const AssemblyListing: React.FC<AssemblyListingProps> = ({
  listingLines,
  currentAddress,
  executionSpeed
}) => {
  // Don't update display if speed is too high
  const shouldHighlight = executionSpeed <= 100;

  return (
    <div className="assembly-listing">
      <h3>Assembly Listing</h3>
      {listingLines.length === 0 ? (
        <p className="listing-empty">No assembly code loaded. Assemble a program to see the listing.</p>
      ) : (
        <table className="listing-table">
          <thead>
            <tr>
              <th>Line</th>
              <th>Address</th>
              <th>OpCode</th>
              <th>Source Code</th>
            </tr>
          </thead>
          <tbody>
            {listingLines.map((line) => {
              const addressNum = parseInt(line.address, 16);
              const isCurrentLine = shouldHighlight && currentAddress !== undefined && addressNum === currentAddress;
              
              return (
                <tr
                  key={line.lineNumber}
                  className={isCurrentLine ? 'current-line' : ''}
                >
                  <td className="line-number">{line.lineNumber}</td>
                  <td className="address">{line.address}</td>
                  <td className="opcode">{line.opCode || ''}</td>
                  <td className="source-code">{line.sourceCode}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
      {executionSpeed > 100 && (
        <p className="speed-warning">
          ⚠ Display frozen at high speed ({executionSpeed} ips). Reduce speed below 100 ips to see highlighting.
        </p>
      )}
    </div>
  );
};

export default AssemblyListing;
